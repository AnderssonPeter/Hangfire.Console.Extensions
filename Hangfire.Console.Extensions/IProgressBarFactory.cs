using Hangfire.Console.Progress;

namespace Hangfire.Console.Extensions
{
    public interface IProgressBarFactory
    {
        IProgressBar Create(string name = null, double value = 0, ConsoleTextColor color = null);
    }
}
