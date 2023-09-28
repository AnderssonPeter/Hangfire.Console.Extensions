using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Hangfire.Annotations;
using System.Threading.Tasks;
using Hangfire.Common;
using System.Collections;

namespace Hangfire
{
    public static class ConsoleRecurringJobManagerExtensions
    {
        public static void AddOrUpdateManuallyTriggered<T>([NotNull] this IRecurringJobManager manager, [NotNull] Expression<Func<T, Task>> methodCall) => 
            manager.AddOrUpdateManuallyTriggered<T>(typeof(T).Name, methodCall);

        public static void AddOrUpdateManuallyTriggered<T>([NotNull] this IRecurringJobManager manager, [NotNull] string recurringJobId, [NotNull] Expression<Func<T, Task>> methodCall)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }

            if (recurringJobId == null)
            {
                throw new ArgumentNullException("recurringJobId");
            }

            if (methodCall == null)
            {
                throw new ArgumentNullException("methodCall");
            }

            Job job = Job.FromExpression(methodCall);
            manager.AddOrUpdate(recurringJobId, job, "0 0 0 31 2 ?");
        }
    }
}
