using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using privatetestrunner.shared.testrunners;
using privatetestrunner.shared.testruns;
using privatetestrunner.shared.options;

namespace privatetestrunner
{
    class Program
    {

        async static Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                                .UseConsoleLifetime()
                                .ConfigureServices((context, services) => {
                                    services.AddOptions<TestRunnerOptions>()
                                        .Bind(context.Configuration.GetSection(TestRunnerOptions.TestRunner));

                                    services.AddHttpClient("test-client")
                                            .AddPolicyHandler(GetRetryPolicy());
                                    services.AddTransient<UrlPingTestRunner>();
                                });

            var host = builder.Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;

                try
                {
                    var testRunnerService = services.GetRequiredService<UrlPingTestRunner>();
                    await testRunnerService.Run();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error Occured - '{ex}'");
                }
            }

            //await host.WaitForShutdownAsync();

        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(20));
        }
    }
}
