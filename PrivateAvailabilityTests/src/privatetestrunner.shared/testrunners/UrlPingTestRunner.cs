using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using privatetestrunner.shared.interfaces;
using privatetestrunner.shared.options;
using privatetestrunner.shared.testruns;
using Azure.Identity;
using Azure;
using Azure.Core;

namespace privatetestrunner.shared.testrunners
{
    public class UrlPingTestRunner : ITestRunner
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions();
        private readonly ILogger _logger;
        private readonly IOptions<TestRunnerOptions> _config;
        private IHttpClientFactory _httpFactory { get; set; }
        public UrlPingTestRunner(ILogger<UrlPingTestRunner> logger, IOptions<TestRunnerOptions> config, IHttpClientFactory httpFactory)
        {
            _logger = logger;
            _config = config;
            try
            {
                TestRunnerOptions options = _config.Value;
            }
            catch (OptionsValidationException ex)
            {
                foreach (string failure in ex.Failures)
                {
                    _logger.LogError(failure);
                }
            }

            _httpFactory = httpFactory;
        }

        public async Task Run()
        {
            // Implementation based on - https://docs.microsoft.com/en-us/azure/azure-monitor/app/availability-azure-functions
            var options = _config.Value;

            // Get the list of Test runs
            TestRuns testRuns = await getTestRuns(options.TestRuns,options.StorageComtainerEndpoint, options.StorageBlobName);


            TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration(options.InstrumentationKey, new InMemoryChannel { EndpointAddress = options.EndpointAddress });
            TelemetryClient telemetryClient = new TelemetryClient(telemetryConfiguration);

            // Obnly run tests for current location
            foreach (var pingTest in testRuns.PingTests.Where(t => t.Locations.Contains(options.Location)))
            {
                _logger.LogInformation($"{DateTime.Now} - Executing availability test run for '{pingTest.Name}'.");

                string operationId = Guid.NewGuid().ToString("N");

                var availability = new AvailabilityTelemetry
                {
                    Id = operationId,
                    Name = pingTest.Name,
                    RunLocation = options.Location,
                    Success = false
                };

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                _logger.LogInformation($"{DateTime.Now} - Testing '{pingTest.Url}'.");
                try
                {
                    // Run Simple Ping test
                    availability.Success = await RunUrlPingTest(pingTest.Url, (HttpStatusCode)pingTest.StatusCode, TimeSpan.FromSeconds(pingTest.Timeout), pingTest.ParseDependentRequests);

                    _logger.LogInformation($"{DateTime.Now} - Successfully executed request.");
                }
                catch (Exception ex)
                {
                    availability.Message = ex.Message;

                    var exceptionTelemetry = new ExceptionTelemetry(ex);
                    exceptionTelemetry.Context.Operation.Id = operationId;
                    exceptionTelemetry.Properties.Add("TestName", pingTest.Name);
                    exceptionTelemetry.Properties.Add("TestTarget", pingTest.Url);
                    exceptionTelemetry.Properties.Add("TestLocation", options.Location);
                    telemetryClient.TrackException(exceptionTelemetry);

                    _logger.LogInformation($"{DateTime.Now} - Failed executing request.");
                }
                finally
                {
                    stopwatch.Stop();

                    availability.Message = "Passed";
                    availability.Duration = stopwatch.Elapsed;
                    availability.Timestamp = DateTimeOffset.UtcNow;

                    telemetryClient.TrackAvailability(availability);
                    // call flush to ensure telemetry is sent
                    telemetryClient.Flush();

                    _logger.LogInformation($"{DateTime.Now} - Test Duration : '{availability.Duration}', TimeStamp: '{availability.Timestamp}'.");
                }

                _logger.LogInformation($"{DateTime.Now} - Exiting availability test run for '{pingTest.Name}'.");
            }

        }

        private async Task<TestRuns> getTestRuns(TestRuns testRuns,string StorageComtainerEndpoint, string blobName)
        {

            if (testRuns != null && 
                testRuns.PingTests.Count > 0)
            {
                _logger.LogInformation($"{DateTime.Now} - Leveraging local test run config.");
                return testRuns;
            }

            if (string.IsNullOrEmpty(StorageComtainerEndpoint) ||
                string.IsNullOrEmpty(blobName))
            {
                _logger.LogError($"{DateTime.Now} - Leveraging remote test run config requires the StorageContainerEndpoint and BlobName configuration to be set.");
                return new TestRuns();
            }
            else
            {
                _logger.LogInformation($"{DateTime.Now} - Leveraging remote test run config from Storage Container Endpoint : '{StorageComtainerEndpoint}', BlobName: '{blobName}'.");

                // Download the Test Run Data
                BlobContainerClient blobContainerClient = new BlobContainerClient(new Uri(StorageComtainerEndpoint), new DefaultAzureCredential());
                BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

                try
                {
                    // Check if the supplied container exists
                    await blobContainerClient.ExistsAsync();
                    // Check if the blob exists
                    await blobClient.ExistsAsync();

                    // download the blob
                    BlobDownloadInfo blob = await blobClient.DownloadAsync();

                    // Deserialize the downloaded JSON
                    return await JsonSerializer.DeserializeAsync<TestRuns>(blob.Content, SerializerOptions);
                }
                catch (RequestFailedException ex)
                {
                    _logger.LogError($"{DateTime.Now} - Failed to load remote test run data.", ex);
                    throw;
                }
            }
        }

        private async Task<Boolean> RunUrlPingTest(string url, HttpStatusCode statusCode, TimeSpan requestTimeout, Boolean parseDependentRequests = false)
        {
            var client = _httpFactory.CreateClient("test-client");
            HttpResponseMessage response;

            // HTTP Request timeout
            client.Timeout = requestTimeout;

            if (!parseDependentRequests)
            {
                response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            }
            else
            {
                response = await client.GetAsync(url);
            }

            Console.WriteLine($"{DateTime.Now} - HTTP Response StatusCode : '{response.StatusCode}'.");

            return (response.StatusCode == statusCode);
        }
    }
}