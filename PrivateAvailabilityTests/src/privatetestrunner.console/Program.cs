using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using privatetestrunner.shared.testrunners;

namespace privatetestrunner
{
    class Program
    {              

        async static Task Main(string[] args)
        {      
            using IHost host = CreateHostBuilder(args)
                                .UseConsoleLifetime()
                                .Build();

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
                    Console.WriteLine("Error Occured");
                }
            }

            //await host.WaitForShutdownAsync();

        }

         static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((HostingAbstractionsHostExtensions, configuration) => {  

            })
            .ConfigureServices((HostBuilderContext, services) => {
                services.AddHttpClient("test-client")
                        .AddPolicyHandler(GetRetryPolicy());
                services.AddTransient<UrlPingTestRunner>();
            });

            static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
            {
                return HttpPolicyExtensions
                            .HandleTransientHttpError()
                            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(20));
            }
    }
}
