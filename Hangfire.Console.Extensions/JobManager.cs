using Hangfire;
using Hangfire.Annotations;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hangfire.Console.Extensions
{

    public class JobManager : IJobManager
    {
        private readonly IMonitoringApi monitoringApi;
        private readonly IPerformingContextAccessor performingContextAccessor;
        private readonly ILogger<JobManager> logger;
        private readonly IBackgroundJobClient backgroundJobClient;

        public JobManager(JobStorage jobStorage, IBackgroundJobClient backgroundJobClient, IPerformingContextAccessor performingContextAccessor, ILogger<JobManager> logger)
        {
            this.monitoringApi = jobStorage.GetMonitoringApi();
            this.performingContextAccessor = performingContextAccessor;
            this.logger = logger;
            this.backgroundJobClient = backgroundJobClient;
        }

        private readonly string[] runningStates = new[] { AwaitingState.StateName, EnqueuedState.StateName, ProcessingState.StateName };

        /// <summary>
        /// Starts a new job and waits for its result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJob"></typeparam>
        /// <param name="methodCall"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TResult> StartWaitAsync<TResult, TJob>([InstantHandle, NotNull] Expression<Func<TJob, Task>> methodCall, CancellationToken cancellationToken = default)
        {
            //todo find a way to mark this job as a Continuation when using backgroundJobClient.Enqueue
            var jobId = backgroundJobClient.Enqueue(methodCall);
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var jobDetails = monitoringApi.JobDetails(jobId);
                var currentState = jobDetails.History.LastOrDefault()?.StateName;
                if (!runningStates.Contains(currentState))
                {
                    if (currentState == SucceededState.StateName)
                        return GetResult<TResult>(jobId);
                    else if (currentState == FailedState.StateName)
                        return ThrowError<TResult>(jobId);
                    else
                        throw new InvalidOperationException($"The job must be in the state '{SucceededState.StateName}' or '{FailedState.StateName}' but is in '{currentState}'");

                }
                await Task.Delay(100, cancellationToken);
            }
        }

        /// <summary>
        /// Starts a new job if we are running inside a job, it marks it as a child.
        /// </summary>
        public void Start<TJob>([InstantHandle] [NotNull] Expression<Action<TJob>> methodCall)
        {
            var context = performingContextAccessor.Get();
            if (context != null)
            {
                backgroundJobClient.ContinueWith(context.BackgroundJob.Id, methodCall);
            }
            else
            {
                backgroundJobClient.Enqueue(methodCall);
            }
        }

        private TResult GetResult<TResult>(string jobId)
        {
            var total = (int)monitoringApi.SucceededListCount();

            var numberOfJobs = 10;
            for (var i = 0; i < total; i += numberOfJobs)
            {
                var start = Math.Max(total - i - numberOfJobs, 0);
                var end = total - i;
                var count = end - start;
                var job = monitoringApi.SucceededJobs(start, count).SingleOrDefault(x => x.Key == jobId).Value;
                if (job != null)
                {
                    var result = job.Result;
                    if (result.GetType() == typeof(string))
                    {
                        return JsonConvert.DeserializeObject<TResult>((string)result);
                    }
                    return (TResult)job.Result;
                }
            }
            throw new InvalidOperationException("Failed to find job");
        }

        private TResult ThrowError<TResult>(string jobId)
        {
            var total = (int)monitoringApi.FailedCount();

            var numberOfJobs = 10;
            for (var i = 0; i < total; i += numberOfJobs)
            {
                var start = Math.Max(total - i - numberOfJobs, 0);
                var end = total - i;
                var count = end - start;
                var job = monitoringApi.FailedJobs(start, count).SingleOrDefault(x => x.Key == jobId).Value;
                if (job != null)
                {
                    throw new JobFailedException($"The job threw a exception of type '{job.ExceptionType}'\nMessage: {job.ExceptionMessage}\nDetails: {job.ExceptionDetails}");
                }
            }
            throw new InvalidOperationException("Failed to find job");
        }
    }
}
