using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.Annotations;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
        private readonly string[] preRunningStates = new[] { EnqueuedState.StateName, ScheduledState.StateName };

        /// <inheritdoc />
        public Task<TResult> StartWaitAsync<TResult, TJob>([InstantHandle, NotNull] Expression<Action<TJob>> methodCall, CancellationToken cancellationToken = default)
        {
            var state = RetrieveContinuationJobState();
            var jobId = backgroundJobClient.Create(methodCall, state);
            return StartAndWaitAsync<TResult>(state, jobId, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TResult> StartWaitAsync<TResult, TJob>([InstantHandle, NotNull] Expression<Func<TJob, Task>> methodCall, CancellationToken cancellationToken = default)
        {
            var state = RetrieveContinuationJobState();
            var jobId = backgroundJobClient.Create(methodCall, state);
            return StartAndWaitAsync<TResult>(state, jobId, cancellationToken);
        }

        /// <inheritdoc />
        public Task StartWaitAsync<TJob>([InstantHandle, NotNull] Expression<Action<TJob>> methodCall, CancellationToken cancellationToken = default)
        {
            var state = RetrieveContinuationJobState();
            var jobId = backgroundJobClient.Create(methodCall, state);
            return StandAndWaitAsync(state, jobId, cancellationToken);
        }

        /// <inheritdoc />
        public Task StartWaitAsync<TJob>([InstantHandle, NotNull] Expression<Func<TJob, Task>> methodCall, CancellationToken cancellationToken = default)
        {
            var state = RetrieveContinuationJobState();
            var jobId = backgroundJobClient.Create(methodCall, state);
            return StandAndWaitAsync(state, jobId, cancellationToken);
        }


        private async Task StandAndWaitAsync(IState state, string jobId, CancellationToken cancellationToken)
        {
            if (state is AwaitingState)
            {
                backgroundJobClient.Requeue(jobId);
            }
            var lastState = ScheduledState.StateName;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var jobDetails = monitoringApi.JobDetails(jobId);
                var currentState = jobDetails.History.OrderBy(h => h.CreatedAt).LastOrDefault()?.StateName;
                if (currentState != lastState)
                {
                    logger.LogDebug("Job changed state from {LastState} to {CurrentState}", lastState, currentState);
                }

                if (preRunningStates.Contains(currentState))
                {
                    if (!preRunningStates.Contains(lastState))
                    {
                        logger.LogDebug("Job was requeued, the job most likely has failed and has retries configured");
                    }
                }
                else if (!runningStates.Contains(currentState))
                {
                    if (currentState == SucceededState.StateName)
                    {
                        return;
                    }
                    else if (currentState == FailedState.StateName)
                    {
                        ThrowError(jobId);
                    }
                    else
                    {
                        throw new InvalidOperationException($"The job must be in the state '{SucceededState.StateName}' or '{FailedState.StateName}' but is in '{currentState}'");
                    }

                }
                lastState = currentState;
                await Task.Delay(100, cancellationToken);
            }
        }

        private async Task<TResult> StartAndWaitAsync<TResult>(IState state, string jobId, CancellationToken cancellationToken)
        {
            if (state is AwaitingState)
            {
                backgroundJobClient.Requeue(jobId);
            }

            var lastState = ScheduledState.StateName;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var jobDetails = monitoringApi.JobDetails(jobId);
                var currentState = jobDetails.History.OrderBy(h => h.CreatedAt).LastOrDefault()?.StateName;
                if (currentState != lastState)
                {
                    logger.LogDebug("Job changed state from {LastState} to {CurrentState}", lastState, currentState);
                }

                if (preRunningStates.Contains(currentState))
                {
                    if (!preRunningStates.Contains(lastState))
                    {
                        logger.LogDebug("Job was requeued, the job most likely has failed and has retries configured");
                    }
                }
                else if (!runningStates.Contains(currentState))
                {
                    if (currentState == SucceededState.StateName)
                    {
                        return GetResult<TResult>(jobId);
                    }
                    else if (currentState == FailedState.StateName)
                    {
                        ThrowError(jobId);
                    }
                    else
                    {
                        throw new InvalidOperationException($"The job must be in the state '{SucceededState.StateName}' or '{FailedState.StateName}' but is in '{currentState}'");
                    }
                }
                lastState = currentState;
                await Task.Delay(100, cancellationToken);
            }
        }

        /// <inheritdoc />
        public string Start<TJob>([InstantHandle][NotNull] Expression<Action<TJob>> methodCall,
            JobContinuationOptions options = JobContinuationOptions.OnAnyFinishedState)
        {
            IState state = RetrieveContinuationJobState(options);

            return backgroundJobClient.Create(methodCall, state);
        }


        /// <inheritdoc />
        public string Start<TJob>([InstantHandle, NotNull] Expression<Func<TJob, Task>> methodCall,
            JobContinuationOptions options = JobContinuationOptions.OnAnyFinishedState)
        {
            IState state = RetrieveContinuationJobState(options);

            return backgroundJobClient.Create(methodCall, state);
        }

        /// <inheritdoc />
        public IState RetrieveContinuationJobState(
            JobContinuationOptions options = JobContinuationOptions.OnAnyFinishedState,
            string queue = "default",
            TimeSpan? expiration = null)
        {
            var context = performingContextAccessor.Get();
            IState enqueuedState = new EnqueuedState(queue);

            if (context != null)
            {
                return expiration.HasValue
                    ? new AwaitingState(context.BackgroundJob.Id, enqueuedState, options, expiration.Value)
                    : new AwaitingState(context.BackgroundJob.Id, enqueuedState, options);
            }

            return enqueuedState;
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
                    if (result == null)
                    {
                        return default;
                    }
                    if (result.GetType() == typeof(string))
                    {
                        return JsonConvert.DeserializeObject<TResult>((string)result);
                    }
                    return (TResult)job.Result;
                }
            }
            throw new InvalidOperationException("Failed to find job");
        }

        private void ThrowError(string jobId)
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
