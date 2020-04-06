using Microsoft.Extensions.Logging;

namespace Hangfire.Console.Extensions
{
    public class HangfireLoggerProvider : ILoggerProvider
    {
        private readonly IPerformingContextAccessor performingContextAccesor;

        public HangfireLoggerProvider(IPerformingContextAccessor performingContextAccesor)
        {
            this.performingContextAccesor = performingContextAccesor;
        }

        public ILogger CreateLogger(string categoryName) =>
            new HangfireLogger(performingContextAccesor);

        public void Dispose()
        {
        }
    }
}
