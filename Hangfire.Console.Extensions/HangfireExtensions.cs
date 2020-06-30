using System.Linq;
using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace Hangfire.Console.Extensions
{
    public static partial class HangfireExtensions
    {
        public static IServiceCollection AddHangfireConsoleExtensions(this IServiceCollection services)
        {
            var loggingProviders = services.Where(x => x.ServiceType == typeof(ILoggerProvider)).ToList();
            foreach(var loggingProvider in loggingProviders)
            {
                System.Console.WriteLine(loggingProvider.ImplementationType);
            }
            services.AddLogging(x => x.AddConfiguration());
            services.AddSingleton<IPerformingContextAccessor, AsyncLocalLogFilter>();
            services.AddSingleton<ILoggerProvider, HangfireLoggerProvider>();
            services.AddSingleton<ICancellationTokenAccessor, CancellationTokenAccessor>();
            services.AddSingleton<IProgressBarFactory, ProgressBarFactory>();
            services.AddSingleton<IJobManager, JobManager>();
            services.AddTransient<IJobCancellationToken>(sp => sp.GetRequiredService<IPerformingContextAccessor>().Get().CancellationToken);
            services.AddTransient<PerformingContext>(sp => sp.GetRequiredService<IPerformingContextAccessor>().Get());
            return services;
        }
    }
}
