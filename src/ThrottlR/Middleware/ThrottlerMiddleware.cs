using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThrottlR
{
    public class ThrottlerMiddleware
    {
        private readonly Func<object, Task> _onResponseStartingDelegate = OnResponseStarting;
        private readonly RequestDelegate _next;
        private readonly IThrottlerService _throttlerService;
        private readonly IThrottlePolicyProvider _throttlePolicyProvider;
        private readonly ICounterKeyBuilder _counterKeyBuilder;
        private readonly ThrottleOptions _options;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<ThrottlerMiddleware> _logger;

        public ThrottlerMiddleware(RequestDelegate next,
           IThrottlerService throttlerService,
           IThrottlePolicyProvider throttlePolicyProvider,
           ICounterKeyBuilder counterKeyBuilder,
           IOptions<ThrottleOptions> options,
           ISystemClock systemClock,
           ILogger<ThrottlerMiddleware> logger)
        {
            _next = next;
            _throttlerService = throttlerService;
            _throttlePolicyProvider = throttlePolicyProvider;
            _counterKeyBuilder = counterKeyBuilder;
            _options = options.Value;
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


            // set RateLimit headers for the longest period
            if (rulesDict?.Count > 0)
            {
                var kvp = rulesDict.OrderByDescending(x => x.Key.TimeWindow).FirstOrDefault();

                var counter = kvp.Value;
                var rule = kvp.Key;

                context.Response.OnStarting(_onResponseStartingDelegate, (rule, counter, context));
            }


            await _next.Invoke(context);
        }

        public Task ReturnQuotaExceededResponse(HttpContext httpContext, ThrottleRule rule, Counter counter)
        {
            var retryAfter = counter.Timestamp + rule.TimeWindow;

            httpContext.Response.Headers["Retry-After"] = retryAfter.ToString("R");

            SetRateLimitHeaders(rule, counter, httpContext);

            httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return _options.OnQuotaExceeded(httpContext, rule, retryAfter);
        }


        private void LogBlockRequest(IThrottleMetadata rateLimitMetadata, string identity, ThrottleRule rule, Counter rateLimitCounter)
        {
            _logger.LogInformation($"Request with identity `{identity}` has been blocked by policy `{rateLimitMetadata.PolicyName}`, quota `{rule.Quota}/{rule.TimeWindow}` exceeded by `{rateLimitCounter.Count}`.");
        }

        private static Task OnResponseStarting(object state)
        {
            var (rule, counter, context) = ((ThrottleRule, Counter, HttpContext))state;

            SetRateLimitHeaders(rule, counter, context);

            return Task.CompletedTask;
        }

        private static void SetRateLimitHeaders(ThrottleRule rule, Counter counter, HttpContext context)
        {
            var remaining = rule.Quota - counter.Count;
            context.Response.Headers["RateLimit-Remaining"] = remaining.ToString();

            var limit = $"{counter.Count}, {rule}";
            context.Response.Headers["RateLimit-Limit"] = limit;

            var reset = counter.Timestamp + rule.TimeWindow;
            context.Response.Headers["RateLimit-Reset"] = reset.ToString();
        }
    }
}
