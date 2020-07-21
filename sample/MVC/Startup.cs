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

            // Adds throttlR services to service collection
            services.AddThrottlR(options => 
            {
                // Configures the default policy
                options.AddDefaultPolicy(policy => 
                {
                    // throttling is based on request ip
                    policy.WithIpResolver()
                        // add general rules for all ips
                        .WithGeneralRule(TimeSpan.FromSeconds(10), 3) // 3 requests could be called every 10 seconds
                        .WithGeneralRule(TimeSpan.FromMinutes(1), 30) // 30 requests could be called every 1 minute
                        .WithGeneralRule(TimeSpan.FromHours(1), 500) // 500 requests could be called every 1 hour

                        // throttling skips "127.0.0.1" & "::1"
                        .WithSafeList("127.0.0.1", "::1")

                        // override general rules for "10.20.10.47" with new rules
                        .WithSpecificRule("10.20.10.47", TimeSpan.FromSeconds(10), 60)
                        .WithSpecificRule("10.20.10.47", TimeSpan.FromMinutes(1), 600) 
                        .WithSpecificRule("10.20.10.47", TimeSpan.FromHours(1), 1000);
                });
            })
            .AddInMemoryCounterStore();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            // Adds Throttler middleware to the pipeline
            app.UseThrottler(); 

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapGet("/hello", context =>
                {
                    return context.Response.WriteAsync("Hello");
                })
                // Throttle "/hello" endpoint with default policy
                .Throttle(); 
            });
        }
    }
}
