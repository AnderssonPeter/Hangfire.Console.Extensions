using Hangfire.Server;

namespace Hangfire.Console.Extensions
{
    public interface IPerformingContextAccessor
    {
        PerformingContext Get();
    }
}
