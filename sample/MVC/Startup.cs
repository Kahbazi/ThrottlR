using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

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
                    policy.WithGeneralRule(TimeSpan.FromSeconds(10), 3);
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
