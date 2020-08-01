# ThrottlR

[![NuGet](https://img.shields.io/nuget/vpre/ThrottlR.svg)](https://www.nuget.org/packages/ThrottlR)

A Throttling middleware for ASP.NET Core.

#### Getting Started

Install [ThrottlR](https://www.nuget.org/packages/ThrottlR) nuget package:

```
dotnet add package ThrottlR
```

Since ThrottlR is implemented on top of Endpoint, ThrottlR middleware needs to be added after `UseRouting()` and before `UseEndpoints()`.

```csharp
public void Configure(IApplicationBuilder app)
{
    app.UseRouting();
    // Adds Throttler middleware to the pipeline
    app.UseThrottler();
    app.UseEndpoints(...);
}
```

Also add ThrottlR to `IServiceCollection`

```csharp
public void ConfigureServices(IServiceCollection services)
{
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

                // override general rules for "10.20.10.47" with new rules
                .WithSpecificRule("10.20.10.47", TimeSpan.FromSeconds(10), 60)
                .WithSpecificRule("10.20.10.47", TimeSpan.FromMinutes(1), 600)
                .WithSpecificRule("10.20.10.47", TimeSpan.FromHours(1), 1000)

                // throttling skips requests coming from IP : "127.0.0.1" or "::1"
                .SafeList.IP("127.0.0.1", "::1")
                // throttling skips requests for User "Admin"
                .SafeList.User("Admin")
                // throttling skips requests with Host header "myApi.local"
                .SafeList.Host("myApi.local");
        });
    })
    .AddInMemoryCounterStore();
}
```

#### How to add ThrottlR on Controller
Add `[Throttle]` Attribute to Controller class or Action. You can add `[DisableThrottle]` for Action that doesn't need throttling.

```csharp
// Throttle this controller with default policy
[Throttle]
[ApiController]
public class ApiController : ControllerBase
{
    [HttpGet("values")]
    public string[] GetValues()
    {
        return new string[] { "value1", "value2" };
    }

    // Override General Rule for this action with 2 requests per second
    [Throttle(PerSecond = 2)]
    [HttpGet("custom")]
    public string[] CustomRule()
    {
        return new string[] { "value1", "value2" };
    }

    // Disable throttle for this action
    [DisableThrottle]
    [HttpGet("other")]
    public string[] GetOtherValues()
    {
        return new string[] { "value1", "value2" };
    }
}
```

#### How to add ThrottlR on Endpoint

Use `Throttle()` extensions method

```csharp
app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/greetings", context =>
    {
        return context.Response.WriteAsync("Greetings");
    })
    // Throttle "/greetings" endpoint with default policy
    .Throttle();

    endpoints.MapGet("/farewell", context =>
    {
        return context.Response.WriteAsync("Farewell");
    })
    // Throttle "/farewell" endpoint and override general rules for default policy
    .Throttle(perSecond: 4);
});
```
