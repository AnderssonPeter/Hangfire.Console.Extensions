using Hangfire.Common;
using System;
using System.Threading;
using Hangfire.Server;
using System.Collections.Generic;

namespace Hangfire.Console.Extensions
{
    /// <summary>
    /// Singelton used to keep track of hangfire jobs
    /// </summary>
    internal class HangfireSubscriber : IServerFilter
    {
        private static readonly AsyncLocal<PerformingContext> localStorage = new AsyncLocal<PerformingContext>();

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
