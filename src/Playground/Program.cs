using Playground;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);


var app = builder.Build();

var options = new PartitionedOptions();
options.AddPolicy<string>("Path", context =>
{
    var endpointName = context.GetEndpoint()?.DisplayName ?? "";
    return new RateLimitPartition<string>(endpointName, name =>
    {
        return new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions(2, QueueProcessingOrder.OldestFirst, 0, TimeSpan.FromMinutes(1)));
    });
});

options.AddPolicy<string>("User", context =>
{
    var userId = context.Request.Query["userId"].ToString();
    return new RateLimitPartition<string>(userId, user =>
    {
        return new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions(7, QueueProcessingOrder.OldestFirst, 0, TimeSpan.FromMinutes(1)));
    });
});

app.UseRateLimiting(options);

app.MapGet("/A", async context =>
{
    await context.Response.WriteAsync(context.Request.Path);
}).RequireRateLimit("Path", "User");

app.MapGet("/B", async context =>
{
    await context.Response.WriteAsync(context.Request.Path);
}).RequireRateLimit("Path", "User");

app.MapGet("/C", async context =>
{
    await context.Response.WriteAsync(context.Request.Path);
}).RequireRateLimit("User");

app.MapGet("/D", async context =>
{
    await context.Response.WriteAsync(context.Request.Path);
}).RequireRateLimit("User").RequireTokenBucketRateLimit(2, TimeSpan.FromSeconds(10), 1);

app.Run();
