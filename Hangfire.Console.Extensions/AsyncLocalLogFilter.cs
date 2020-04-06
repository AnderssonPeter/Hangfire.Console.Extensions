using System;
using System.Threading;
using Hangfire.Server;

namespace Hangfire.Console.Extensions
{
    /// <summary>
    /// Used to hook into the Hangfire lifetime events and store the context for the current async task.
    /// </summary>
    public class AsyncLocalLogFilter : IServerFilter, IDisposable, IPerformingContextAccessor
    {
        private readonly AsyncLocal<PerformingContext> localStorage = new AsyncLocal<PerformingContext>();

        public AsyncLocalLogFilter()
        {
            GlobalJobFilters.Filters.Add(this);
        }

        public void OnPerforming(PerformingContext filterContext)
        {
            localStorage.Value = filterContext;
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            localStorage.Value = null;
        }

        public void Dispose()
        {
            GlobalJobFilters.Filters.Remove(this);
        }

        public PerformingContext Get()
        {
            return localStorage.Value;
        }
    }
}
