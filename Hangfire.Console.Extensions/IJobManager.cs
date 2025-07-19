using Hangfire.Annotations;
using Hangfire.States;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Hangfire.Console.Extensions
{
    public interface IJobManager
    {
        Task<TResult> StartWaitAsync<TResult, TJob>([InstantHandle, NotNull] Expression<Func<TJob, Task>> methodCall, CancellationToken cancellationToken = default);
        Task StartWaitAsync<TJob>([InstantHandle, NotNull] Expression<Func<TJob, Task>> methodCall, CancellationToken cancellationToken = default);
        string Start<TJob>([InstantHandle][NotNull] Expression<Action<TJob>> methodCall, JobContinuationOptions options = JobContinuationOptions.OnAnyFinishedState);
        string Start<TJob>([InstantHandle, NotNull] Expression<Func<TJob, Task>> methodCall, JobContinuationOptions options = JobContinuationOptions.OnAnyFinishedState);
        IState RetrieveContinuationJobState(JobContinuationOptions options = JobContinuationOptions.OnAnyFinishedState, string queue = "default", TimeSpan? expiration = null);
    }
}
