using System.Threading.Tasks;
using Hangfire.Console.Extensions;
using Microsoft.Extensions.Logging;

namespace Sample
{
    public class ContinuationJob
    {
        private readonly ILogger<SampleJob> logger;
        private readonly IProgressBarFactory progressBarFactory;

        public ContinuationJob(ILogger<SampleJob> logger, IProgressBarFactory progressBarFactory)
        {
            this.logger = logger;
            this.progressBarFactory = progressBarFactory;
        }

        public async Task RunAsync()
        {
            logger.LogTrace("Test");
            logger.LogDebug("Test");
            logger.LogInformation("Test");
            logger.LogWarning("Test");
            logger.LogError("Test");
            logger.LogCritical("Test");

            var progress = progressBarFactory.Create("Test");
            for (var i = 0; i < 100; i++)
            {
                progress.SetValue(i + 1);
                await Task.Delay(100);
            }
        }
    }
}
