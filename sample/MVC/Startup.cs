using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ThrottlR;

namespace MVC
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddThrottlR(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithIpResolver() // throttling is based on request ip
                        .WithGeneralRule(TimeSpan.FromSeconds(10), 3) // 3 requests could be called every 10 seconds
                        .WithGeneralRule(TimeSpan.FromMinutes(1), 30) // 30 requests could be called every 1 minute
                        .WithGeneralRule(TimeSpan.FromHours(1), 500) // 500 requests could be called every 1 hour
                        .WithSafeList("127.0.0.1", "::1"); // throttling skips "127.0.0.1" & "::1"
                });
            })
            .AddInMemoryCounterStore();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseThrottler();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/values", context =>
                {
                    return context.Response.WriteAsync("values");
                })
                .Throttle();
            });
        }
    }
}
