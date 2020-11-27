using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThrottlR.Internal.AsyncKeyLock;
using ThrottlR.Models;
using ThrottlR.Policy;
using ThrottlR.Service.Store;

namespace ThrottlR.Service
{
    public class ThrottlerService : IThrottlerService
    {
        /// The key-lock used for limiting requests.
        private static readonly AsyncKeyLock _asyncLock = new ();

        private readonly ICounterStore _counterStore;
        private readonly ISystemClock _systemClock;

        public ThrottlerService(ICounterStore counterStore, ISystemClock systemClock)
        {
            _counterStore = counterStore;
            _systemClock = systemClock;
        }

        public IEnumerable<ThrottleRule> GetRules(IReadOnlyList<ThrottleRule> generalRules,
            IReadOnlyList<ThrottleRule> specificRules)
        {

            _ = generalRules ?? throw new ArgumentNullException(nameof(generalRules));
            _ = specificRules ?? throw new ArgumentNullException(nameof(specificRules));

            var generalRuleIndex = 0;
            var specificRuleIndex = 0;

            while (true)
            {
                if (specificRuleIndex == specificRules.Count && generalRuleIndex == generalRules.Count)
                {
                    break;
                }
                else if (specificRuleIndex == specificRules.Count)
                {
                    for (; generalRuleIndex < generalRules.Count; generalRuleIndex++)
                    {
                        yield return generalRules[generalRuleIndex];
                    }
                    break;
                }
                else if (generalRuleIndex == generalRules.Count)
                {
                    for (; specificRuleIndex < specificRules.Count; specificRuleIndex++)
                    {
                        yield return specificRules[specificRuleIndex];
                    }
                    break;
                }

                var generalRule = generalRules[generalRuleIndex];
                var speceficRule = specificRules[specificRuleIndex];

                if (speceficRule.TimeWindow > generalRule.TimeWindow)
                {
                    yield return speceficRule;
                    specificRuleIndex++;
                }
                else if (speceficRule.TimeWindow < generalRule.TimeWindow)
                {
                    yield return generalRule;
                    generalRuleIndex++;
                }
                else
                {
                    yield return speceficRule;
                    specificRuleIndex++;
                    generalRuleIndex++;
                }
            }
        }

        public async Task<Counter> ProcessRequestAsync(ThrottlerItem throttlerItem, ThrottleRule rule,
            CancellationToken cancellationToken)
        {
            Counter counter;

            using (await _asyncLock.WriterLockAsync(throttlerItem.GenerateCounterKey()))
            {
                var entry = await _counterStore.GetAsync(throttlerItem, cancellationToken);
                var utcNow = _systemClock.UtcNow;

                // entry is not null has not expired
                if (entry is not null && entry.Value.Timestamp + rule.TimeWindow >= utcNow)
                {
                    // create new counter with incremented count
                    counter = new (entry.Value.Timestamp, entry.Value.Count + 1);
                }
                // else create a new counter
                else
                {
                    // create new counter with base count
                    counter = new (utcNow, 1);
                }

                await _counterStore.SetAsync(throttlerItem, counter, rule.TimeWindow, cancellationToken);
            }

            return counter;
        }
    }
}
