using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Console.Extensions;
using Hangfire.Server;
using Microsoft.Extensions.Logging;

namespace SampleWithSerilog
{
    public class ContinuationJob
    {
        private readonly ILogger<SampleJob> logger;
        private readonly IProgressBarFactory progressBarFactory;
        private readonly PerformingContext performingContext;

        public ContinuationJob(ILogger<SampleJob> logger, IProgressBarFactory progressBarFactory, PerformingContext performingContext)
        {
            this.logger = logger;
            this.progressBarFactory = progressBarFactory;
            this.performingContext = performingContext;
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
    }
}
