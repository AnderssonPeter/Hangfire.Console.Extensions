using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Extensions;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SampleWithSerilog
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire((serviceProvider, configuration) => configuration
                .UseConsole()
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                //.UseSqlServerStorage(@"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;"));
                .UseMemoryStorage());
            services.AddHangfireConsoleExtensions();
            services.AddHangfireServer();

            services.AddTransient<SampleJob>();
            services.AddTransient<ContinuationJob>();
            services.AddTransient<ResultJob>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IJobManager jobManager, IRecurringJobManager recurringJobManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseHangfireDashboard();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("<a href=\"\\hangfire\\\">Dashboard</a>");
                });
                endpoints.MapGet("/startAndWait", async context =>
                {
                    var jobManager = context.RequestServices.GetRequiredService<IJobManager>();
                    await jobManager.StartWaitAsync<ContinuationJob>(t => t.RunAsync());
                });
                endpoints.MapGet("/startAndWaitWithResult", async context =>
                {
                    var jobManager = context.RequestServices.GetRequiredService<IJobManager>();
                    var result = await jobManager.StartWaitAsync<int, ResultJob>(t => t.RunAsync());
                    await context.Response.WriteAsync("Your lucky number might not be: " + result);
                });
            });

            jobManager.Start<SampleJob>(x => x.RunAsync());
            recurringJobManager.AddOrUpdate<ContinuationJob>("test", job => job.RunAsync(), "0 0 * ? * *");
            recurringJobManager.AddOrUpdateManuallyTriggered<ResultJob>(job => job.RunAsync());
        }
    }
}
