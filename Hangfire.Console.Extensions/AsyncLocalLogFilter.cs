using Hangfire.Server;

namespace Hangfire.Console.Extensions
{
    /// <summary>
    /// Used to hook into the Hangfire lifetime events and store the context for the current async task.
    /// </summary>
    public class AsyncLocalLogFilter : IPerformingContextAccessor
    {
        public PerformingContext Get()
        {
            return HangfireSubscriber.Value;
        }
    }
}
