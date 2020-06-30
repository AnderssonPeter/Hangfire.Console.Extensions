using System;
using Serilog;
using Serilog.Configuration;

namespace Hangfire.Console.Extensions.Serilog
{
    public static class HangfireSinkExtensions
    {
        public static LoggerConfiguration WithHangfireContext(
            this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));
            return enrichmentConfiguration.With<HangfireConsoleSerilogEnricher>();
        }

        public static LoggerConfiguration Hangfire(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new HangfireSink(formatProvider));
        }
    }
}
