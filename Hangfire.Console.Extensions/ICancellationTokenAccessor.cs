using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace Hangfire.Console.Extensions
{
    public interface ICancellationTokenAccessor
    {
        CancellationToken Get();
    }
}
