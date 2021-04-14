using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace privatetestrunner
{
    public class TestRunner
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private IHttpClientFactory _httpFactory { get; set; }
        public TestRunner(ILogger<TestRunner> logger,IConfiguration config, IHttpClientFactory httpFactory)
        {
            _logger = logger;
            _config = config;
            _httpFactory = httpFactory;
        }
 
        public async Task Run()
        {
            // Implementation based on - https://docs.microsoft.com/en-us/azure/azure-monitor/app/availability-azure-functions
            TestRunOptions options = new();
            _config.GetSection(nameof(TestRunOptions.TestRun))
                    .Bind(options);

            TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration(options.InstrumentationKey, new InMemoryChannel { EndpointAddress = options.EndpointAddress });
            TelemetryClient telemetryClient = new TelemetryClient(telemetryConfiguration);

             foreach (var pingTest in options.PingTests)
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
                    availability.Success = await RunUrlPingTest(pingTest.Url, pingTest.StatusCode, TimeSpan.FromSeconds(pingTest.Timeout), pingTest.ParseDependentRequests);
                    // Or Multi-step WebTest
                    // availability.Success = await RunMultiStepWebTest();

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

        private async Task<Boolean> RunUrlPingTest(string url, HttpStatusCode statusCode, TimeSpan requestTimeout, Boolean parseDependentRequests = false)
        {
            var client = _httpFactory.CreateClient();
            HttpResponseMessage response;

            // HTTP Request timeout
            client.Timeout = requestTimeout;

            if(!parseDependentRequests)
            {
                response =  await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            }
            else
            {
                response = await client.GetAsync(url);
            }

            Console.WriteLine($"{DateTime.Now} - HTTP Response StatusCode : '{response.StatusCode}'.");
            
            return (response.StatusCode == statusCode);
        }

        private async Task<Boolean> RunMultiStepWebTest()
        {
            var client = _httpFactory.CreateClient();
            throw new NotImplementedException("Implement RunMultiStepWebTest!");
        }

    }
}