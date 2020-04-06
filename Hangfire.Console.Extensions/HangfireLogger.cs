using System;
using Microsoft.Extensions.Logging;

namespace Hangfire.Console.Extensions
{
    public class HangfireLogger : ILogger
    {
        private readonly IPerformingContextAccessor contextAccessor;

        public HangfireLogger(IPerformingContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        private ConsoleTextColor GetConsoleColor(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    return ConsoleTextColor.Red;
                case LogLevel.Error:
                    return ConsoleTextColor.Yellow;
                case LogLevel.Warning:
                    return ConsoleTextColor.DarkYellow;
                case LogLevel.Information:
                    return ConsoleTextColor.White;
                case LogLevel.Debug:
                    return ConsoleTextColor.DarkGray;
                case LogLevel.Trace:
                    return ConsoleTextColor.Gray;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return "trce";
                case LogLevel.Debug:
                    return "dbug";
                case LogLevel.Information:
                    return "info";
                case LogLevel.Warning:
                    return "warn";
                case LogLevel.Error:
                    return "fail";
                case LogLevel.Critical:
                    return "crit";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var context = contextAccessor.Get();
            if (context != null)
            {
                var message = $"{GetLogLevelString(logLevel)}: {formatter(state, exception)}";
                if (exception == null)
                {
                    context.WriteLine(GetConsoleColor(logLevel), message);
                }
                else
                {
                    context.WriteLine(GetConsoleColor(logLevel), message + Environment.NewLine + exception.ToString());
                }
            }
        }
    }
}
