using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottlR
{
    public class DistributedCacheCounterStore : ICounterStore
    {
        private readonly IDistributedCache _cache;

        public DistributedCacheCounterStore(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async ValueTask SetAsync(ThrottlerItem throttlerItem, Counter counter, TimeSpan? expirationTime, CancellationToken cancellationToken)
        {
            var options = new DistributedCacheEntryOptions();

            if (expirationTime.HasValue)
            {
                options.SetAbsoluteExpiration(expirationTime.Value);
            }

            var key = GenerateThrottlerItemKey(throttlerItem);
            await _cache.SetStringAsync(key, Serialize(counter), options, cancellationToken);
        }

        public async ValueTask<Counter?> GetAsync(ThrottlerItem throttlerItem, CancellationToken cancellationToken)
        {
            var key = GenerateThrottlerItemKey(throttlerItem);
            var stored = await _cache.GetStringAsync(key, cancellationToken);

            if (!string.IsNullOrEmpty(stored))
            {
                return Deserialize(stored);
            }

            return default;
        }

        public async ValueTask RemoveAsync(ThrottlerItem throttlerItem, CancellationToken cancellationToken)
        {
            var key = GenerateThrottlerItemKey(throttlerItem);

            await _cache.RemoveAsync(key, cancellationToken);
        }

        public virtual string GenerateThrottlerItemKey(ThrottlerItem throttlerItem)
        {
            return throttlerItem.GenerateCounterKey("Throttler");
        }

        private static string Serialize(Counter counter)
        {
            return $"{counter.Timestamp.Ticks},{counter.Count}";
        }

        private static Counter? Deserialize(string stored)
        {
            try
            {
                var items = stored.Split(',');

                var timestamp = new DateTime(Convert.ToInt64(items[0]));
                var count = Convert.ToInt32(items[1]);

                return new Counter(timestamp, count);
            }
            catch
            {
                return default;
            }
        }
    }
}
