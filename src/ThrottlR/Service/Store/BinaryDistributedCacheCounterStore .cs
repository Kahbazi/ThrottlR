using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using ThrottlR.Models;

namespace ThrottlR.Service.Store
{
    public class BinaryDistributedCacheCounterStore : ICounterStore
    {
        private readonly IDistributedCache _cache;

        public BinaryDistributedCacheCounterStore(IDistributedCache cache)
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
            await _cache.SetAsync(key, ToBinary(counter).ToArray(), options, cancellationToken);
        }

        public async ValueTask<Counter?> GetAsync(ThrottlerItem throttlerItem, CancellationToken cancellationToken)
        {
            var key = throttlerItem.GenerateCounterKey();
            var stored = await _cache.GetAsync(key, cancellationToken);

            if (stored is not null)
            {
                return FromBinary(stored);
            }

            return default;
        }

        public async ValueTask RemoveAsync(ThrottlerItem throttlerItem, CancellationToken cancellationToken)
        {
            var key = throttlerItem.GenerateCounterKey();
            await _cache.RemoveAsync(key, cancellationToken);
        }

        private static ReadOnlySpan<byte> ToBinary(Counter counter)
        {

            var data = new byte[12];
            Buffer.BlockCopy(BitConverter.GetBytes(counter.Count), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(counter.Timestamp.Ticks), 0, data, 4, 8);
            return data;
        }

        private static Counter? FromBinary(byte[] counterBinaryData)
        {
            try
            {
                var count = BitConverter.ToInt32(counterBinaryData.AsSpan(0, 4));
                var timestamp = new DateTime(BitConverter.ToInt64(counterBinaryData.AsSpan(4, 8)));

                return new Counter(timestamp, count);
            }
            catch
            {
                return default;
            }
        }
    }

}
