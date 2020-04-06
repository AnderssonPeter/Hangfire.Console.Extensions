using Hangfire.Annotations;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Hangfire.Console.Extensions
{
    public interface IJobManager
    {
        Task<TResult> StartWaitAsync<TResult, TJob>([InstantHandle][NotNull] Expression<Func<TJob, Task>> methodCall, CancellationToken cancellationToken = default);
        void Start<TJob>([InstantHandle] [NotNull] Expression<Action<TJob>> methodCall);
    }
}
