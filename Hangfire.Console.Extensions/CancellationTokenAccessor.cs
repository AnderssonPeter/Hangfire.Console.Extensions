using System.Threading;

namespace Hangfire.Console.Extensions
{
    /// <summary>
    /// Used to get access to the Hangfire CancellationToken
    /// </summary>
    public class CancellationTokenAccessor : ICancellationTokenAccessor
    {
        private readonly IPerformingContextAccessor performingContextAccessor;

        public CancellationTokenAccessor(IPerformingContextAccessor performingContextAccessor)
        {
            this.performingContextAccessor = performingContextAccessor;
        }

        public CancellationToken Get()
        {
            return performingContextAccessor.Get()?.CancellationToken?.ShutdownToken ?? CancellationToken.None;
        }
    }
}
