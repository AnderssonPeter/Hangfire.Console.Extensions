using System.Collections.Generic;
using System.Linq;
using Hangfire.Server;
using Serilog.Events;

namespace Hangfire.Console.Extensions.Serilog
{
    public class PerformingContextStructureValue : StructureValue
    {
        public PerformingContextStructureValue(PerformingContext performingContext) : base(CreateProperties(performingContext))
        {
            PerformingContext = performingContext;
        }

        private static IEnumerable<LogEventProperty> CreateProperties(PerformingContext performingContext)
        {
            var properties = new List<LogEventProperty>()
            {
                new LogEventProperty("Id", new ScalarValue(performingContext.BackgroundJob.Id)),
                new LogEventProperty("CreatedAt", new ScalarValue(performingContext.BackgroundJob.CreatedAt))
            };
            if (performingContext.BackgroundJob.Job != null)
            {
                properties.Add(new LogEventProperty("Type", new ScalarValue(performingContext.BackgroundJob.Job.Method.DeclaringType.Name)));
                properties.Add(new LogEventProperty("Method", new ScalarValue(performingContext.BackgroundJob.Job.Method.Name)));
                properties.Add(new LogEventProperty("Arguments", new SequenceValue(performingContext.BackgroundJob.Job.Args.Select(x => GetScalarValue(x)))));
            }
            return properties;
        }

        private static ScalarValue GetScalarValue(object value)
        {
            if (value != null && !value.GetType().IsPrimitive)
            {
                value = value.ToString();
            }
            return new ScalarValue(value);
        }

        internal PerformingContext PerformingContext { get; }
    }
}
