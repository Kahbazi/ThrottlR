using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using ThrottlR.Models;

namespace ThrottlR.Service.Store
{
    public class StringDistributedCacheCounterStore : ICounterStore
    {
        private readonly IDistributedCache _cache;

        public StringDistributedCacheCounterStore(IDistributedCache cache)
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

            var key = throttlerItem.GenerateCounterKey();
            await _cache.SetStringAsync(key, Serialize(counter), options, cancellationToken);
        }

        public async ValueTask<Counter?> GetAsync(ThrottlerItem throttlerItem, CancellationToken cancellationToken)
        {
            var key = throttlerItem.GenerateCounterKey();
            var stored = await _cache.GetStringAsync(key, cancellationToken);

            if (stored is not null)
            {
                return Deserialize(stored);
            }

            return default;
        }

        public async ValueTask RemoveAsync(ThrottlerItem throttlerItem, CancellationToken cancellationToken)
        {
            var key = throttlerItem.GenerateCounterKey();
            await _cache.RemoveAsync(key, cancellationToken);
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
