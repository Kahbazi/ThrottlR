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

        public async ValueTask SetAsync(string key, Counter counter, TimeSpan? expirationTime, CancellationToken cancellationToken)
        {
            var options = new DistributedCacheEntryOptions();

            if (expirationTime.HasValue)
            {
                options.SetAbsoluteExpiration(expirationTime.Value);
            }

            await _cache.SetStringAsync(key, Serialize(counter), options, cancellationToken);
        }

        public async ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken)
        {
            var stored = await _cache.GetStringAsync(key, cancellationToken);

            return !string.IsNullOrEmpty(stored);
        }

        public async ValueTask<Counter?> GetAsync(string key, CancellationToken cancellationToken)
        {
            var stored = await _cache.GetStringAsync(key, cancellationToken);

            if (!string.IsNullOrEmpty(stored))
            {
                return Deserialize(stored);
            }

            return default;
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

        public async ValueTask RemoveAsync(string key, CancellationToken cancellationToken)
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
    }
}