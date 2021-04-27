using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using privatetestrunner.shared.testrunners;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;
using System;
using Microsoft.Extensions.Configuration;
using System.IO;
using privatetestrunner.shared.options;

[assembly: FunctionsStartup(typeof(privatetestrunner.function.Startup))]

namespace privatetestrunner.function
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {

            builder.Services
                .AddOptions<TestRunOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection(TestRunOptions.TestRun).Bind(settings);
                });

            //builder.Services.AddOptions<TestRunOptions>()
            //                            .Bind(builder.GetContext().Configuration.GetSection(TestRunOptions.TestRun));

            builder.Services
                .AddHttpClient("test-client"); //.AddPolicyHandler(GetRetryPolicy());

            builder.Services.AddTransient<UrlPingTestRunner>();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath,"appsettings.json"), optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(20));
        }
    }
}
