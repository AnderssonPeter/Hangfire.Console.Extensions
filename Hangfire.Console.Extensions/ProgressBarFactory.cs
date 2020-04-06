using Hangfire.Console.Progress;

namespace Hangfire.Console.Extensions
{
    public class ProgressBarFactory : IProgressBarFactory
    {
        private readonly IPerformingContextAccessor performingContextAccessor;

        public ProgressBarFactory(IPerformingContextAccessor performingContextAccessor)
        {
            this.performingContextAccessor = performingContextAccessor;
        }

        public IProgressBar Create(string name = null, double value = 0, ConsoleTextColor color = null)
        {
            return performingContextAccessor.Get().WriteProgressBar(name: name, value: value, color: color);
        }
    }
}
