using System.Threading;
using System.Threading.Tasks;

namespace ThrottlR
{
    public class ThrottlerService : IThrottlerService
    {
        private readonly ICounterStore _counterStore;
        private readonly ISystemClock _systemClock;

        public ThrottlerService(
           ICounterStore counterStore,
           ISystemClock systemClock)
        {
            _counterStore = counterStore;
            _systemClock = systemClock;
        }

        /// The key-lock used for limiting requests.
        private static readonly AsyncKeyLock _asyncLock = new AsyncKeyLock();

        public async Task<Counter> ProcessRequestAsync(string counterId, ThrottleRule rule, CancellationToken cancellationToken)
        {
            Counter counter;

            // serial reads and writes on same key
            using (await _asyncLock.WriterLockAsync(counterId).ConfigureAwait(false))
            {
                var entry = await _counterStore.GetAsync(counterId, cancellationToken);

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

                // stores: id (string) - timestamp (datetime) - total_requests (long)
                await _counterStore.SetAsync(counterId, counter, rule.TimeWindow, cancellationToken);
            }

            return counter;
        }

        
    }
}
