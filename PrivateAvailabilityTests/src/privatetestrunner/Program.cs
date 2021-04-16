using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

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
