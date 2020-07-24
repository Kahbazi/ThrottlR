using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThrottlR
{
    public class ThrottlerMiddleware
    {
        private static readonly byte[] _exceededQuoataMessage = Encoding.UTF8.GetBytes("You have exceeded your quota.");

        private readonly Func<object, Task> _onResponseStartingDelegate = OnResponseStarting;
        private readonly RequestDelegate _next;
        private readonly IThrottlerService _throttlerService;
        private readonly IThrottlePolicyProvider _throttlePolicyProvider;
        private readonly ICounterKeyBuilder _counterKeyBuilder;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<ThrottlerMiddleware> _logger;

        public ThrottlerMiddleware(RequestDelegate next,
           IThrottlerService throttlerService,
           IThrottlePolicyProvider throttlePolicyProvider,
           ICounterKeyBuilder counterKeyBuilder,
           ISystemClock systemClock,
           ILogger<ThrottlerMiddleware> logger)
        {
            _next = next;
            _throttlerService = throttlerService;
            _throttlePolicyProvider = throttlePolicyProvider;
            _counterKeyBuilder = counterKeyBuilder;
            _systemClock = systemClock;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var endpoint = context.GetEndpoint();

            var throttleMetadata = endpoint?.Metadata.GetMetadata<IThrottleMetadata>();
            if (throttleMetadata == null)
            {
                await _next.Invoke(context);
                return;
            }

            var disableThrottle = endpoint?.Metadata.GetMetadata<IDisableThrottle>();
            if (disableThrottle != null)
            {
                await _next.Invoke(context);
                return;
            }

            var policy = await _throttlePolicyProvider.GetPolicyAsync(throttleMetadata.PolicyName);
            if (policy.Resolver == null)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return;
            }

            // compute identity from request
            var identity = await policy.Resolver.ResolveAsync(context);

            // check safe list
            if (policy.SafeList.Contains(identity))
            {
                await _next.Invoke(context);
                return;
            }


            var rules = _throttlerService.GetRules(policy, identity);

            Dictionary<ThrottleRule, Counter> rulesDict = null;

            foreach (var rule in rules)
            {
                var counterId = _counterKeyBuilder.Build(identity, rule, throttleMetadata.PolicyName, endpoint.DisplayName);

                // increment counter
                var counter = await _throttlerService.ProcessRequestAsync(counterId, rule, context.RequestAborted);

                if (rule.Quota > 0)
                {
                    // check if key expired
                    if (counter.Timestamp + rule.TimeWindow < _systemClock.UtcNow)
                    {
                        continue;
                    }

                    // check if limit is reached
                    if (counter.Count > rule.Quota)
                    {
                        LogBlockRequest(throttleMetadata, identity, rule, counter);

                        await ReturnQuotaExceededResponse(context, rule, counter);

                        return;
                    }
                }

                (rulesDict ?? new Dictionary<ThrottleRule, Counter>()).Add(rule, counter);
            }


            // set X-Rate-Limit headers for the longest period
            if (rulesDict?.Count > 0)
            {
                var rule = rulesDict.OrderByDescending(x => x.Key.TimeWindow).FirstOrDefault();

                var counter = rule.Value;
                var quotaPolicy = rule.Key;

                context.Response.OnStarting(_onResponseStartingDelegate, (quotaPolicy, counter, context));
            }


            await _next.Invoke(context);
        }

        public Task ReturnQuotaExceededResponse(HttpContext httpContext, ThrottleRule quotaPolicy, Counter counter)
        {
            var retryAfter = counter.Timestamp + quotaPolicy.TimeWindow;

            httpContext.Response.Headers["Retry-After"] = retryAfter.ToString("R");

            SetRateLimitHeaders(quotaPolicy, counter, httpContext);

            httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            httpContext.Response.ContentType = "text/plain";

            return httpContext.Response.Body.WriteAsync(_exceededQuoataMessage, 0, _exceededQuoataMessage.Length);
        }


        private void LogBlockRequest(IThrottleMetadata rateLimitMetadata, string identity, ThrottleRule quotaPolicy, Counter rateLimitCounter)
        {
            _logger.LogInformation($"Request with identity `{identity}` has been blocked by policy `{rateLimitMetadata.PolicyName}`, quota `{quotaPolicy.Quota}/{quotaPolicy.TimeWindow}` exceeded by `{rateLimitCounter.Count}`.");
        }

        private static Task OnResponseStarting(object state)
        {
            var (quotaPolicy, counter, context) = ((ThrottleRule, Counter, HttpContext))state;

            SetRateLimitHeaders(quotaPolicy, counter, context);

            return Task.CompletedTask;
        }

        private static void SetRateLimitHeaders(ThrottleRule quotaPolicy, Counter counter, HttpContext context)
        {
            var remaining = quotaPolicy.Quota - counter.Count;
            context.Response.Headers["RateLimit-Remaining"] = remaining.ToString();

            var limit = $"{counter.Count}, {quotaPolicy}";
            context.Response.Headers["RateLimit-Limit"] = limit;

            var reset = counter.Timestamp + quotaPolicy.TimeWindow;
            context.Response.Headers["RateLimit-Reset"] = reset.ToString();
        }
    }
}
