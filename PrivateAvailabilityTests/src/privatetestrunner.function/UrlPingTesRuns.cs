using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using privatetestrunner.shared.testrunners;

namespace privatetestrunner.function
{
    public class UrlPingTesRuns
    {
        private readonly UrlPingTestRunner testRunnerService;

        public UrlPingTesRuns(UrlPingTestRunner testRunnerService)
        {
            this.testRunnerService = testRunnerService;
        }

        [FunctionName("TestRuns")]
        public async Task Run([TimerTrigger("*/15 * * * *")]TimerInfo myTimer, ILogger log)
        {
            try
            {
                await this.testRunnerService.Run();
            }
            catch (Exception ex)
            {
                log.LogError($"Error Occured", ex);
            }
        }
    }
}
