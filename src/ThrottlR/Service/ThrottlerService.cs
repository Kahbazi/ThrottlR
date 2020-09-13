using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottlR
{
    public class ThrottlerService : IThrottlerService
    {
        /// The key-lock used for limiting requests.
        private static readonly AsyncKeyLock _asyncLock = new AsyncKeyLock();

        private readonly ICounterStore _counterStore;
        private readonly ISystemClock _systemClock;

        public ThrottlerService(ICounterStore counterStore, ISystemClock systemClock)
        {
            _counterStore = counterStore;
            _systemClock = systemClock;
        }

        public IEnumerable<ThrottleRule> GetRules(IReadOnlyList<ThrottleRule> generalRules, IReadOnlyList<ThrottleRule> specificRules)
        {       
            var g = 0;
            var s = 0;

            while (true)
            {
                if (s == specificRules.Count && g == generalRules.Count)
                {
                    break;
                }
                else if (s == specificRules.Count)
                {
                    for (; g < generalRules.Count; g++)
                    {
                        yield return generalRules[g];
                    }
                    break;
                }
                else if (g == generalRules.Count)
                {
                    for (; s < specificRules.Count; s++)
                    {
                        yield return specificRules[s];
                    }
                    break;
                }

                var generalRule = generalRules[g];
                var speceficRule = specificRules[s];

                if (speceficRule.TimeWindow > generalRule.TimeWindow)
                {
                    yield return speceficRule;
                    s++;
                }
                else if (speceficRule.TimeWindow < generalRule.TimeWindow)
                {
                    yield return generalRule;
                    g++;
                }
                else
                {
                    yield return speceficRule;
                    s++;
                    g++;
                }
            }
        }

        public async Task<Counter> ProcessRequestAsync(ThrottlerItem throttlerItem, ThrottleRule rule, CancellationToken cancellationToken)
        {
            Counter counter;

            using (await _asyncLock.WriterLockAsync(throttlerItem.GenerateCounterKey()))
            {
                var entry = await _counterStore.GetAsync(throttlerItem, cancellationToken);

                if (entry.HasValue)
                {
                    // entry has not expired
                    if (entry.Value.Timestamp + rule.TimeWindow >= _systemClock.UtcNow)
                    {
                        // increment request count
                        var totalCount = entry.Value.Count + 1;

                        counter = new Counter(entry.Value.Timestamp, totalCount);
                    }
                    else
                    {
                        counter = new Counter(_systemClock.UtcNow, 1);
                    }
                }
                else
                {
                    counter = new Counter(_systemClock.UtcNow, 1);
                }

                await _counterStore.SetAsync(throttlerItem, counter, rule.TimeWindow, cancellationToken);
            }

            return counter;
        }
    }
}
