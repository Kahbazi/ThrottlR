using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThrottlR
{
    public class ThrottlerMiddleware
    {
        private static readonly TimeSpan _oneSecond = TimeSpan.FromSeconds(1);

        private readonly RequestDelegate _next;
        private readonly ThrottlerService _throttlerService;
        private readonly ThrottleOptions _options;
        private readonly IThrottlePolicyProvider _throttlePolicyProvider;
        private readonly ICounterKeyBuilder _counterKeyBuilder;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<ThrottlerMiddleware> _logger;

        public ThrottlerMiddleware(RequestDelegate next,
            IOptions<ThrottleOptions> options,
            ThrottlerService rateLimitProcessor,
            IThrottlePolicyProvider throttlePolicyProvider,
            ICounterKeyBuilder counterKeyBuilder,
            ISystemClock systemClock,
            ILogger<ThrottlerMiddleware> logger)
        {
            _next = next;
            _options = options.Value;
            _throttlerService = rateLimitProcessor;
            _throttlePolicyProvider = throttlePolicyProvider;
            _counterKeyBuilder = counterKeyBuilder;
            _systemClock = systemClock;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var endpoint = context.GetEndpoint();

            var rateLimitMetadata = endpoint?.Metadata.GetMetadata<IThrottleMetadata>();
            if (rateLimitMetadata == null)
            {
                await _next.Invoke(context);
                return;
            }

            var policy = await _throttlePolicyProvider.GetPolicyAsync(rateLimitMetadata.PolicyName);
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

            policy.SpecificRules.TryGetValue(identity, out var specificRules);

            var rules = _throttlerService.CombineRules(policy.GeneralRules, specificRules);

            var rulesDict = new Dictionary<ThrottleRule, RateLimitCounter>();

            foreach (var rule in rules)
            {
                var counterId = _counterKeyBuilder.Build(identity, rule, rateLimitMetadata.PolicyName);

                // increment counter
                var rateLimitCounter = await _throttlerService.ProcessRequestAsync(identity, rule, context.RequestAborted);

                if (rule.Limit > 0)
                {
                    // check if key expired
                    if (rateLimitCounter.Timestamp + rule.Period < _systemClock.UtcNow)
                    {
                        continue;
                    }

                    // check if limit is reached
                    if (rateLimitCounter.Count > rule.Limit)
                    {
                        //compute retry after value
                        var retryAfter = RetryAfterFrom(rateLimitCounter.Timestamp, rule);

                        // log blocked request
                        LogBlockRequest(rateLimitMetadata, identity, rule, rateLimitCounter);

                        // break execution
                        await ReturnQuotaExceededResponse(context, rule, retryAfter);

                        return;
                    }
                }
                // if limit is zero or less, block the request.
                else
                {
                    // log blocked request
                    LogBlockRequest(rateLimitMetadata, identity, rule, rateLimitCounter);

                    // break execution (TimeSpan max used to represent infinity)
                    await ReturnQuotaExceededResponse(context, rule, TimeSpan.MaxValue);

                    return;
                }

                rulesDict.Add(rule, rateLimitCounter);
            }

            // set X-Rate-Limit headers for the longest period
            if (rulesDict.Count > 0 && !_options.DisableRateLimitHeaders)
            {
                var rule = rulesDict.OrderByDescending(x => x.Key.Period).FirstOrDefault();
                var headers = _throttlerService.GetRateLimitHeaders(rule.Value, rule.Key);

                context.Response.OnStarting(SetRateLimitHeaders, (headers, context));
            }

            await _next.Invoke(context);
        }

        public Task ReturnQuotaExceededResponse(HttpContext httpContext, ThrottleRule rule, TimeSpan retryAfter)
        {
            var message = $"API calls quota exceeded! maximum admitted {rule.Limit} per {rule.Period}, retry after {retryAfter}.";

            if (!_options.DisableRateLimitHeaders)
            {
                httpContext.Response.Headers["Retry-After"] = $"{retryAfter:F0}";
            }

            httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            httpContext.Response.ContentType = "text/plain";

            return httpContext.Response.WriteAsync(message);
        }

        public TimeSpan RetryAfterFrom(DateTime timestamp, ThrottleRule rule)
        {
            var diff = timestamp + rule.Period - _systemClock.UtcNow;

            if (diff > _oneSecond)
            {
                return diff;
            }
            else
            {
                return _oneSecond;
            }
        }

        private void LogBlockRequest(IThrottleMetadata rateLimitMetadata, string identity, ThrottleRule rule, RateLimitCounter rateLimitCounter)
        {
            _logger.LogInformation($"Request with identity {identity} has been blocked, quota {rule.Limit}/{rule.Period} exceeded by {rateLimitCounter.Count}. Blocked by policy {rateLimitMetadata.PolicyName}.");
        }

        private Task SetRateLimitHeaders(object rateLimitHeaders)
        {
            var (headers, context) = ((RateLimitHeaders headers, HttpContext context))rateLimitHeaders;
            
            context.Response.Headers["X-Rate-Limit-Limit"] = headers.Limit;// better be 00:10:00
            context.Response.Headers["X-Rate-Limit-Remaining"] = headers.Remaining;
            context.Response.Headers["X-Rate-Limit-Reset"] = headers.Reset;

            return Task.CompletedTask;
        }
    }
}
