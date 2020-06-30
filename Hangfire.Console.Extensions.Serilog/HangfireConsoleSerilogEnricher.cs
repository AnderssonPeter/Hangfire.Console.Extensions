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


        /// <inheritdoc />
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            // Create property value with PerformContext and put as "PerformContext"
            var prop = new LogEventProperty(
                "PerformContext", new PerformContextValue { PerformContext = asyncLocalLogFilter.Get() }
            );
            logEvent.AddOrUpdateProperty(prop);
        }
    }
}
