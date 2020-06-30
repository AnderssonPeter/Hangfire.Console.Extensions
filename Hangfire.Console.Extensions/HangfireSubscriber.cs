using System;
using System.Threading;
using Hangfire.Server;

namespace Hangfire.Console.Extensions
{
    /// <summary>
    /// Singelton used to keep track of hangfire jobs
    /// </summary>
    internal class HangfireSubscriber : IServerFilter
    {
        private static readonly HangfireSubscriber instance;
        private static readonly AsyncLocal<PerformingContext> localStorage = new AsyncLocal<PerformingContext>();

        static HangfireSubscriber()
        {
            instance = new HangfireSubscriber();
            GlobalJobFilters.Filters.Add(instance);
        }

        public static PerformingContext Value => localStorage.Value;

        public void OnPerforming(PerformingContext filterContext)
        {
            localStorage.Value = filterContext;
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            localStorage.Value = null;
        }
    }
}
