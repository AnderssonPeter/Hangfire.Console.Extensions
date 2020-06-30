using System;
using Hangfire.Console;
using Hangfire.Logging;
using Serilog.Core;
using Serilog.Events;

namespace Hangfire.Console.Extensions.Serilog
{
    public class HangfireSink : ILogEventSink
    {
        private readonly IFormatProvider formatProvider;

        public HangfireSink(IFormatProvider formatProvider)
        {
            this.formatProvider = formatProvider;
        }

        private static string GetLogLevelString(LogEventLevel logLevel)
        {
            switch (logLevel)
            {
                case LogEventLevel.Verbose:
                    return "trce";
                case LogEventLevel.Debug:
                    return "dbug";
                case LogEventLevel.Information:
                    return "info";
                case LogEventLevel.Warning:
                    return "warn";
                case LogEventLevel.Error:
                    return "fail";
                case LogEventLevel.Fatal:
                    return "crit";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        private ConsoleTextColor GetConsoleColor(LogEventLevel logLevel)
        {
            switch (logLevel)
            {
                case LogEventLevel.Fatal:
                    return ConsoleTextColor.Red;
                case LogEventLevel.Error:
                    return ConsoleTextColor.Yellow;
                case LogEventLevel.Warning:
                    return ConsoleTextColor.DarkYellow;
                case LogEventLevel.Information:
                    return ConsoleTextColor.White;
                case LogEventLevel.Debug:
                    return ConsoleTextColor.DarkGray;
                case LogEventLevel.Verbose:
                    return ConsoleTextColor.Gray;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent.Properties.TryGetValue("PerformContext", out var logEventPerformContext))
            {
                // Get the object reference from our custom property
                var performContext = (logEventPerformContext as PerformContextValue)?.PerformContext;

                // And write the line on it
                performContext?.WriteLine(GetConsoleColor(logEvent.Level), GetLogLevelString(logEvent.Level) + ": " + logEvent.RenderMessage(formatProvider));
            }
        }
    }
}
