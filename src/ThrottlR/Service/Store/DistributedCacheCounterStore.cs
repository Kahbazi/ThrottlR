using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

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
            await _cache.SetAsync(key, Serialize(counter), options, cancellationToken);
        }

        public async ValueTask<Counter?> GetAsync(ThrottlerItem throttlerItem, CancellationToken cancellationToken)
        {
            var key = GenerateThrottlerItemKey(throttlerItem);
            var stored = await _cache.GetAsync(key, cancellationToken);

            if (stored != null)
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

        private static byte[] Serialize(Counter counter)
        {
            var data = new byte[12];

            // no need to check for success, it only fails on size mismatch which shouldn't happen
            BitConverter.TryWriteBytes(data.AsSpan(0, 4), counter.Count);
            BitConverter.TryWriteBytes(data.AsSpan(4, 8), counter.Timestamp.Ticks);
            return data;
        }

        private static Counter? Deserialize(byte[] counterBinaryData)
        {
            try
            {
                var count = BitConverter.ToInt32(counterBinaryData, 0);
                var timestamp = new DateTime(BitConverter.ToInt64(counterBinaryData, 4));
                return new Counter(timestamp, count);
            }
            catch
            {
                return default;
            }
        }
    }
}
