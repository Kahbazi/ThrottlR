using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ThrottlR.Tests;
using Xunit;

namespace ThrottlR
{
    public class ThrottlerMiddlewareTests
    {
        [Fact]
        public async Task Skips_When_There_Is_No_Endpoint()
        {
            // Arrange
            var (next, _, _, middleware, context) = Create();

            // Act
            await middleware.Invoke(context);


            // Assert
            Assert.True(next.Called);
        }

        [Fact]
        public async Task Skips_When_There_Is_No_ThrottleMetadata()
        {
            // Arrange
            var (next, _, _, middleware, context) = Create();

            var endpoint = CreateEndpoint();
            context.SetEndpoint(endpoint);

            // Act
            await middleware.Invoke(context);


            // Assert
            Assert.True(next.Called);
        }

        [Fact]
        public async Task Returns_500_When_There_Is_No_Policy()
        {
            // Arrange
            var (next, _, _, middleware, context) = Create();

            var endpoint = CreateEndpoint(new EnableThrottle());
            context.SetEndpoint(endpoint);

            // Act
            await middleware.Invoke(context);


            // Assert
            Assert.False(next.Called);
            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        }

        [Fact]
        public async Task Returns_500_When_There_Is_No_Resolver()
        {
            // Arrange
            var (next, _, throttleOptions, middleware, context) = Create();

            var endpoint = CreateEndpoint(new EnableThrottle());
            context.SetEndpoint(endpoint);

            throttleOptions.AddDefaultPolicy(new ThrottlePolicy { Resolver = null });

            // Act
            await middleware.Invoke(context);


            // Assert
            Assert.False(next.Called);
            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        }

        [Fact]
        public async Task Skips_When_Identity_Is_In_SafeList()
        {
            // Arrange
            var (next, timeMachine, throttleOptions, middleware, context) = Create();

            var endpoint = CreateEndpoint(new EnableThrottle());
            context.SetEndpoint(endpoint);
            context.Connection.RemoteIpAddress = new IPAddress(new byte[] { 10, 20, 10, 47 });

            throttleOptions.AddDefaultPolicy(x => x.SafeList.IP("10.20.10.47")
                                                   .WithGeneralRule(TimeSpan.FromSeconds(1), 1));

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(next.Called);
        }

        [Fact]
        public async Task Quota_Exceeds_In_A_Time_Window()
        {
            // Arrange
            var (next, timeMachine, throttleOptions, middleware, context) = Create();

            var endpoint = CreateEndpoint(new EnableThrottle());
            context.SetEndpoint(endpoint);

            throttleOptions.AddDefaultPolicy(x => x.WithGeneralRule(TimeSpan.FromSeconds(1), 1));

            await middleware.Invoke(context);

            Assert.True(next.Called);

            next.Called = false;


            await middleware.Invoke(context);

            Assert.False(next.Called);
            Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
            Assert.Equal("text/plain", context.Response.ContentType);
        }

        [Fact]
        public async Task Custom_Quota_Exceeds_Delegate()
        {
            // Arrange
            var (next, timeMachine, throttleOptions, middleware, context) = Create();

            var endpoint = CreateEndpoint(new EnableThrottle());
            context.SetEndpoint(endpoint);

            var cuotaExceededDelegateCalled = false;
            throttleOptions.AddDefaultPolicy(x => x.WithGeneralRule(TimeSpan.FromSeconds(1), 1));
            throttleOptions.OnQuotaExceeded = (HttpContext httpContext, ThrottleRule rule, DateTime retryAfter) =>
            {
                cuotaExceededDelegateCalled = true;
                return Task.CompletedTask;
            };

            await middleware.Invoke(context);

            Assert.True(next.Called);

            next.Called = false;


            await middleware.Invoke(context);

            Assert.False(next.Called);
            Assert.True(cuotaExceededDelegateCalled);
            Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
        }

        [Fact]
        public async Task Quota_Exceeds_In_A_Time_Windows_Skips_In_The_Next_Window()
        {
            // Arrange
            var (next, timeMachine, throttleOptions, middleware, context) = Create();

            var endpoint = CreateEndpoint(new EnableThrottle());
            context.SetEndpoint(endpoint);

            throttleOptions.AddDefaultPolicy(x => x.WithGeneralRule(TimeSpan.FromSeconds(1), 1));

            // 00:00:00.0
            await middleware.Invoke(context);

            Assert.True(next.Called);


            // 00:00:00.0
            next.Called = false;
            await middleware.Invoke(context);

            Assert.False(next.Called);
            Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);


            // 00:00:01.1
            timeMachine.Travel(TimeSpan.FromMilliseconds(1001));
            next.Called = false;
            await middleware.Invoke(context);

            Assert.True(next.Called);
        }

        [Fact]
        public async Task Skips_When_There_Is_ThrottleMetadata_And_DisableThrottle()
        {
            // Arrange
            var (next, _, _, middleware, context) = Create();

            var endpoint = CreateEndpoint(new EnableThrottle(), DisableThrottle.Instance);
            context.SetEndpoint(endpoint);

            await middleware.Invoke(context);

            // Assert
            Assert.True(next.Called);
        }

        [Fact]
        public async Task PerEndpointPolicy()
        {
            // Arrange
            var (next, timeMachine, throttleOptions, middleware, context) = Create();

            throttleOptions.AddDefaultPolicy(x => x.WithGeneralRule(TimeSpan.FromSeconds(1), 1)
                                                   .ApplyPerEndpoint());

            // 00:00:00.0
            var endpoint1 = CreateEndpoint("Endpoint-1", new EnableThrottle());
            context.SetEndpoint(endpoint1);

            await middleware.Invoke(context);

            Assert.True(next.Called);


            // 00:00:00.0
            next.Called = false;
            await middleware.Invoke(context);

            Assert.False(next.Called);
            Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);


            // 00:00:00.0
            var endpoint2 = CreateEndpoint("Endpoint-2", new EnableThrottle());
            context.SetEndpoint(endpoint2);

            await middleware.Invoke(context);

            Assert.True(next.Called);


            // 00:00:00.0
            next.Called = false;
            await middleware.Invoke(context);

            Assert.False(next.Called);
            Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
        }

        [Fact]
        public async Task ThrottleAttribute_Override_GeneralRule()
        {
            // Arrange
            var (next, timeMachine, throttleOptions, middleware, context) = Create();

            var endpoint = CreateEndpoint(new ThrottleAttribute { PerSecond = 2 });
            context.SetEndpoint(endpoint);

            throttleOptions.AddDefaultPolicy(x => x.WithGeneralRule(TimeSpan.FromSeconds(1), 1));

            // 00:00:00.0
            await middleware.Invoke(context);

            Assert.True(next.Called);


            // 00:00:00.0
            next.Called = false;
            await middleware.Invoke(context);

            Assert.True(next.Called);
        }

        [Fact]
        public async Task ThrottleAttribute_With_Policy()
        {
            // Arrange
            var (next, timeMachine, throttleOptions, middleware, context) = Create();

            var endpoint = CreateEndpoint(new ThrottleAttribute { PerSecond = 1, PolicyName = "policy-1" });
            context.SetEndpoint(endpoint);

            throttleOptions.AddPolicy("policy-1", builder => { });

            // 00:00:00.0
            await middleware.Invoke(context);

            Assert.True(next.Called);


            // 00:00:00.0
            next.Called = false;
            await middleware.Invoke(context);

            Assert.False(next.Called);
            Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
        }

        private (Result next, TimeMachine timeMachine, ThrottleOptions throttleOptions, ThrottlerMiddleware middleware, HttpContext context) Create()
        {
            var result = new Result { Called = false };
            Task Next(HttpContext context)
            {
                result.Called = true;
                return Task.CompletedTask;
            }

            var timeMachine = new TimeMachine();
            var throttleOptions = new ThrottleOptions();

            var middleware = CreateMiddleware(throttleOptions, timeMachine, Next);

            var context = new DefaultHttpContext();
            var endpoint = CreateEndpoint();
            context.SetEndpoint(endpoint);

            return (result, timeMachine, throttleOptions, middleware, context);
        }

        public class Result
        {
            public bool Called { get; set; }
        }

        private Endpoint CreateEndpoint(params object[] throttleMetadata)
        {
            return CreateEndpoint(string.Empty, throttleMetadata);
        }

        private Endpoint CreateEndpoint(string endpointName = "", params object[] throttleMetadata)
        {
            return new Endpoint(context => Task.CompletedTask, new EndpointMetadataCollection((IEnumerable<object>)throttleMetadata), endpointName);
        }

        public ThrottlerMiddleware CreateMiddleware(ThrottleOptions throttleOptions, TimeMachine timeMachine, RequestDelegate next)
        {
            var options = Options.Create(throttleOptions);
            var throttlerService = CreateThrottleService(timeMachine);
            var throttlePolicyProvider = new DefaultThrottlePolicyProvider(options);
            var middleware = new ThrottlerMiddleware(next, throttlerService, throttlePolicyProvider, options, timeMachine, NullLogger<ThrottlerMiddleware>.Instance);

            return middleware;
        }

        private static ThrottlerService CreateThrottleService(TimeMachine timeMachine)
        {
            var store = new TestCounterStore();
            var throttlerService = new ThrottlerService(store, timeMachine);

            return throttlerService;
        }
    }
}
