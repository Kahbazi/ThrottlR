using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace ThrottlR
{
    public class RateLimitProcessor : ThrottlerService
    {
        private readonly ThrottleOptions _options;
        private readonly IRateLimitStore _counterStore;
        private readonly ISystemClock _systemClock;

        public RateLimitProcessor(
           IOptions<ThrottleOptions> options,
           IRateLimitStore counterStore,
           ISystemClock systemClock)
        {
            _options = options.Value;
            _counterStore = counterStore;
            _systemClock = systemClock;
        }

        /// The key-lock used for limiting requests.
        private static readonly AsyncKeyLock _asyncLock = new AsyncKeyLock();

        public bool IsSafe(string identity, IReadOnlyList<string> safeList)
        {
            if (safeList != null && safeList.Contains(identity))
            {
                return true;
            }

            return false;
        }

        public async Task<RateLimitCounter> ProcessRequestAsync(string counterId, ThrottleRule rule, CancellationToken cancellationToken)
        {
            RateLimitCounter counter;

            // serial reads and writes on same key
            using (await _asyncLock.WriterLockAsync(counterId).ConfigureAwait(false))
            {
                var entry = await _counterStore.GetAsync(counterId, cancellationToken);

                if (entry.HasValue)
                {
                    // entry has not expired
                    if (entry.Value.Timestamp + rule.Period >= _systemClock.UtcNow)
                    {
                        // increment request count
                        var totalCount = entry.Value.Count + 1;

                        counter = new RateLimitCounter(entry.Value.Timestamp, totalCount);
                    }
                    else
                    {
                        counter = new RateLimitCounter(_systemClock.UtcNow, 1);
                    }
                }
                else
                {
                    counter = new RateLimitCounter(_systemClock.UtcNow, 1);
                }

                // stores: id (string) - timestamp (datetime) - total_requests (long)
                await _counterStore.SetAsync(counterId, counter, rule.Period, cancellationToken);
            }

            return counter;
        }

        public RateLimitHeaders GetRateLimitHeaders(RateLimitCounter counter, ThrottleRule rule)
        {
            var remaining = rule.Limit - counter.Count;
            var reset = counter.Timestamp + rule.Period;

            var headers = new RateLimitHeaders
            {
                Reset = reset.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffzz", DateTimeFormatInfo.InvariantInfo),
                Limit = rule.Period.ToString(),
                Remaining = remaining.ToString()
            };

            return headers;
        }

        public List<ThrottleRule> CombineRules(IReadOnlyList<ThrottleRule> generalRules, IReadOnlyList<ThrottleRule> speceficRules)
        {
            //TODO: Reduce allocation

            var limits = new List<ThrottleRule>();

            if (speceficRules != null)
            {
                // get the most restrictive limit for each period 
                limits = speceficRules.GroupBy(l => l.Period)
                    .Select(l => l.OrderBy(x => x.Limit))
                    .Select(l => l.First())
                    .ToList();
            }

            // get the most restrictive general limit for each period 
            var generalLimits = generalRules
                .GroupBy(l => l.Period)
                .Select(l => l.OrderBy(x => x.Limit))
                .Select(l => l.First())
                .ToList();

            foreach (var generalLimit in generalLimits)
            {
                // add general rule if no specific rule is declared for the specified period
                if (!limits.Exists(l => l.Period == generalLimit.Period))
                {
                    limits.Add(generalLimit);
                }
            }

            limits = limits.OrderBy(l => l.Period).ToList();

            if (_options.StackBlockedRequests)
            {
                limits.Reverse();
            }

            return limits;
        }
    }
}
