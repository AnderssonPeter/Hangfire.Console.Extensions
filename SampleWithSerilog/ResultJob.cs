using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console.Extensions;
using Hangfire.Server;
using Microsoft.Extensions.Logging;

namespace SampleWithSerilog
{
    public class ResultJob
    {
        private readonly ILogger<ResultJob> logger;
        private readonly IProgressBarFactory progressBarFactory;
        private readonly PerformingContext performingContext;
        private readonly IJobManager jobManager;
        private readonly IJobCancellationToken jobCancellationToken;

        public ResultJob(ILogger<ResultJob> logger, IJobCancellationToken jobCancellationToken, IProgressBarFactory progressBarFactory, PerformingContext performingContext, IJobManager jobManager)
        {
            this.logger = logger;
            this.progressBarFactory = progressBarFactory;
            this.performingContext = performingContext;
            this.jobManager = jobManager;
            this.jobCancellationToken = jobCancellationToken;
        }

        [JobDisplayName("ResultJob")]
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(timeoutInSeconds: 60 * 60 * 3)]
        public async Task<int> RunAsync()
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
