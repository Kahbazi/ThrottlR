using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ThrottlR
{
    public class ThrottlerMiddleware
    {
        private static readonly IReadOnlyList<ThrottleRule> _emptyRules = Array.Empty<ThrottleRule>();
        private static readonly Func<object, Task> _onResponseStartingDelegate = OnResponseStarting;

        private readonly RequestDelegate _next;
        private readonly IThrottlerService _throttlerService;
        private readonly IThrottlePolicyProvider _throttlePolicyProvider;
        private readonly ThrottleOptions _options;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<ThrottlerMiddleware> _logger;

        public ThrottlerMiddleware(RequestDelegate next,
           IThrottlerService throttlerService,
           IThrottlePolicyProvider throttlePolicyProvider,
           IOptions<ThrottleOptions> options,
           ISystemClock systemClock,
           ILogger<ThrottlerMiddleware> logger)
        {
            _next = next;
            _throttlerService = throttlerService;
            _throttlePolicyProvider = throttlePolicyProvider;
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
            if (policy == null)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return;
            }

            if (policy.Resolver == null)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return;
            }

            var scope = await policy.Resolver.ResolveAsync(context);
            var isSafe = await CheckSafeScopes(context, policy, scope);
            if (isSafe)
            {
                await _next.Invoke(context);
                return;
            }

            IReadOnlyList<ThrottleRule> generalRules;
            if (throttleMetadata is IThrottleRulesMetadata throttle)
            {
                // ThrottleRuleMetadata overrides the general rule
                generalRules = throttle.GeneralRules;
            }
            else
            {
                generalRules = policy.GeneralRules;
            }

            IReadOnlyList<ThrottleRule> specificRules;
            if (policy.SpecificRules.TryGetValue(scope, out var specificRulesList))
            {
                specificRules = specificRulesList;
            }
            else
            {
                specificRules = _emptyRules;
            }

            var rules = _throttlerService.GetRules(generalRules, specificRules);

            (ThrottleRule rule, Counter counter, bool hasBeenSet) longestRule = (default, default, false);

            foreach (var rule in rules)
            {
                var throttlerItem = new ThrottlerItem(rule, throttleMetadata.PolicyName, scope, endpoint.DisplayName);

                // increment counter
                var counter = await _throttlerService.ProcessRequestAsync(throttlerItem, rule, context.RequestAborted);

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
                        LogBlockRequest(throttleMetadata, scope, rule, counter);

                        await ReturnQuotaExceededResponse(context, rule, counter);

                        return;
                    }
                }

                longestRule = (rule, counter, true);
            }

            // set RateLimit headers for the longest period
            if (longestRule.hasBeenSet)
            {
                context.Response.OnStarting(_onResponseStartingDelegate, (longestRule.rule, longestRule.counter, context));
            }

            await _next.Invoke(context);
        }

        private static async Task<bool> CheckSafeScopes(HttpContext context, ThrottlePolicy policy, string scope)
        {
            // check safe list
            foreach (var kvp in policy.SafeList)
            {
                var safeList = kvp.Value;
                if (safeList.Count == 0)
                {
                    continue;
                }

                var resolver = kvp.Key;
                string safeScope;
                if (policy.Resolver == resolver)
                {
                    safeScope = scope;
                }
                else
                {
                    safeScope = await resolver.ResolveAsync(context);
                }

                for (var i = 0; i < safeList.Count; i++)
                {
                    var safe = safeList[i];
                    if (resolver.Matches(safeScope, safe))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private Task ReturnQuotaExceededResponse(HttpContext httpContext, ThrottleRule rule, Counter counter)
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
