using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Server;
using Serilog.Core;
using Serilog.Events;

namespace Hangfire.Console.Extensions.Serilog
{
    class HangfireConsoleSerilogEnricher : ILogEventEnricher
    {
        private readonly AsyncLocalLogFilter asyncLocalLogFilter = new AsyncLocalLogFilter();

        public HangfireConsoleSerilogEnricher()
        {
        }

        private static StructureValue CreateValue(PerformingContext performingContext)
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
            return new StructureValue(properties);
        }

        private static ScalarValue GetScalarValue(object value)
        {
            if (value != null && !value.GetType().IsPrimitive)
            {
                value = value.ToString();
            }
            return new ScalarValue(value);
        }

        /// <inheritdoc />
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var context = asyncLocalLogFilter.Get();
            if (context == null)
                return;
            // Create property value with PerformContext and put as "PerformContext"
            var property = new LogEventProperty("HangFireJob", CreateValue(context));
            logEvent.AddOrUpdateProperty(property);
        }
    }
}
