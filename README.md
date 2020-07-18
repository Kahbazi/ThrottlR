# ThrottlR

[![NuGet](https://img.shields.io/nuget/vpre/ThrottlR.svg)](https://www.nuget.org/packages/ThrottlR)

#### Installing ThrottlR

Install [ThrottlR](https://www.nuget.org/packages/ThrottlR) nuget package:

```
dotnet add package ThrottlR
```

Add ThrottlR to `IServiceCollection`

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddThrottlR(options =>
    {
        //configure options
    })
    .AddInMemoryCounterStore();
}
```

#### How to add ThrottlR on Controller
Add `[Throttle]` Attribute to Controller class or Action.

```csharp
[Throttle]
[ApiController]
public class ApiController : ControllerBase
{
    [HttpGet("values")]
    public string[] GetValues()
    {
        return new string[] { "value1", "value2" };
    }
}
```

You can add `[DisableThrottle]` for Action that doesn't need throttling.

```csharp
[Throttle]
[ApiController]
public class ApiController : ControllerBase
{
    [HttpGet("values")]
    public string[] GetValues()
    {
        return new string[] { "value1", "value2" };
    }

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
    endpoints.MapGet("/values", context =>
    {
        return context.Response.WriteAsync("values");
    })
    .Throttle();
});
```
