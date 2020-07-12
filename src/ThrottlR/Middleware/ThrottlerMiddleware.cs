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
        private static readonly byte[] _exceededQuoataMessage = Encoding.UTF8.GetBytes("You have exceeded your quota.");
        private static readonly TimeSpan _oneSecond = TimeSpan.FromSeconds(1);

        private readonly Func<object, Task> _onResponseStartingDelegate = OnResponseStarting;
        private readonly RequestDelegate _next;
        private readonly IThrottlerService _throttlerService;
        private readonly ThrottleOptions _options;
        private readonly IThrottlePolicyProvider _throttlePolicyProvider;
        private readonly ICounterKeyBuilder _counterKeyBuilder;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<ThrottlerMiddleware> _logger;

        public ThrottlerMiddleware(RequestDelegate next,
            IOptions<ThrottleOptions> options,
            IThrottlerService throttlerService,
            IThrottlePolicyProvider throttlePolicyProvider,
            ICounterKeyBuilder counterKeyBuilder,
            ISystemClock systemClock,
            ILogger<ThrottlerMiddleware> logger)
        {
            _next = next;
            _options = options.Value;
            _throttlerService = throttlerService;
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

            var rules = CombineRules(policy.GeneralRules, specificRules);

            var rulesDict = new Dictionary<ThrottleRule, RateLimitCounter>();

            for (var i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                var counterId = _counterKeyBuilder.Build(identity, rule, rateLimitMetadata.PolicyName);

                // increment counter
                var counter = await _throttlerService.ProcessRequestAsync(identity, rule, context.RequestAborted);

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
                        LogBlockRequest(rateLimitMetadata, identity, rule, counter);

                        await ReturnQuotaExceededResponse(context, rule, counter);

                        return;
                    }
                }

                rulesDict.Add(rule, counter);
            }

            // set X-Rate-Limit headers for the longest period
            if (rulesDict.Count > 0)
            {
                var rule = rulesDict.OrderByDescending(x => x.Key.TimeWindow).FirstOrDefault();

                var counter = rule.Value;
                var quotaPolicy = rule.Key;

                context.Response.OnStarting(_onResponseStartingDelegate, (quotaPolicy, counter, context));
            }

            await _next.Invoke(context);
        }

        public Task ReturnQuotaExceededResponse(HttpContext httpContext, ThrottleRule quotaPolicy, RateLimitCounter counter)
        {
            var retryAfter = counter.Timestamp + quotaPolicy.TimeWindow;

            httpContext.Response.Headers["Retry-After"] = retryAfter.ToString("R");

            SetRateLimitHeaders(quotaPolicy, counter, httpContext);

            httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            httpContext.Response.ContentType = "text/plain";

            return httpContext.Response.Body.WriteAsync(_exceededQuoataMessage, 0, _exceededQuoataMessage.Length);
        }

        public List<ThrottleRule> CombineRules(IReadOnlyList<ThrottleRule> generalRules, IReadOnlyList<ThrottleRule> speceficRules)
        {
            //TODO: Reduce allocation

            var limits = new List<ThrottleRule>();

            if (speceficRules != null)
            {
                // get the most restrictive limit for each period 
                limits = speceficRules.GroupBy(l => l.TimeWindow)
                    .Select(l => l.OrderBy(x => x.Quota))
                    .Select(l => l.First())
                    .ToList();
            }

            // get the most restrictive general limit for each period 
            var generalLimits = generalRules
                .GroupBy(l => l.TimeWindow)
                .Select(l => l.OrderBy(x => x.Quota))
                .Select(l => l.First())
                .ToList();

            foreach (var generalLimit in generalLimits)
            {
                // add general rule if no specific rule is declared for the specified period
                if (!limits.Exists(l => l.TimeWindow == generalLimit.TimeWindow))
                {
                    limits.Add(generalLimit);
                }
            }

            limits = limits.OrderBy(l => l.TimeWindow).ToList();

            if (_options.StackBlockedRequests)
            {
                limits.Reverse();
            }

            return limits;
        }

        private void LogBlockRequest(IThrottleMetadata rateLimitMetadata, string identity, ThrottleRule quotaPolicy, RateLimitCounter rateLimitCounter)
        {
            _logger.LogInformation($"Request with identity `{identity}` has been blocked by policy `{rateLimitMetadata.PolicyName}`, quota `{quotaPolicy.Quota}/{quotaPolicy.TimeWindow}` exceeded by `{rateLimitCounter.Count}`.");
        }

        private static Task OnResponseStarting(object state)
        {
            var (quotaPolicy, counter, context) = ((ThrottleRule, RateLimitCounter, HttpContext))state;

            SetRateLimitHeaders(quotaPolicy, counter, context);

            return Task.CompletedTask;
        }

        private static void SetRateLimitHeaders(ThrottleRule quotaPolicy, RateLimitCounter counter, HttpContext context)
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
