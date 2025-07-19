using Hangfire;
using Hangfire.Console.Extensions;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sample
{
    public class ContinuationJob
    {
        private readonly ILogger<ContinuationJob> logger;
        private readonly IProgressBarFactory progressBarFactory;
        private readonly PerformingContext performingContext;
        private readonly IJobCancellationToken jobCancellationToken;

        public ContinuationJob(ILogger<ContinuationJob> logger, IProgressBarFactory progressBarFactory, PerformingContext performingContext, IJobCancellationToken jobCancellationToken)
        {
            this.logger = logger;
            this.progressBarFactory = progressBarFactory;
            this.performingContext = performingContext;
            this.jobCancellationToken = jobCancellationToken;
        }

        public void Run()
        {
            logger.LogInformation("JobId: {JobId}", performingContext.BackgroundJob.Id);
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
                Thread.Sleep(100);
            }
        }

        public async Task RunAsync()
        {
            logger.LogInformation("JobId: {JobId}", performingContext.BackgroundJob.Id);
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

        public async Task<int> RunWithReturnAsync()
        {
            logger.LogInformation("JobId: {JobId}", performingContext.BackgroundJob.Id);
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

            return new Random().Next(10, 10000);
        }
    }
}
