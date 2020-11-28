using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace ThrottlR
{
    public class DistributedCacheCounterStore : ICounterStore
    {
        private readonly IDistributedCache _cache;
        private const int I32Len = 4;
        private const int I64Len = 8;

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

            var key = throttlerItem.GenerateCounterKey();
            await _cache.SetAsync(key, ToBinary(counter).ToArray(), options, cancellationToken);
        }

        public async ValueTask<Counter?> GetAsync(ThrottlerItem throttlerItem, CancellationToken cancellationToken)
        {
            var key = throttlerItem.GenerateCounterKey();
            var stored = await _cache.GetAsync(key, cancellationToken);

            if (stored != null)
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

            var data = new byte[I32Len + I64Len];
            Buffer.BlockCopy(BitConverter.GetBytes(counter.Count), default, data, default, I32Len);
            Buffer.BlockCopy(BitConverter.GetBytes(counter.Timestamp.Ticks), default, data, I32Len, I64Len);
            return data;
        }

        private static Counter? FromBinary(byte[] counterBinaryData)
        {
            try
            {

                var counterBinaryDataAsSpan = counterBinaryData.AsSpan();
                var count = BitConverter.ToInt32(counterBinaryDataAsSpan.Slice(default, I32Len));
                var timestamp = new DateTime(BitConverter.ToInt64(counterBinaryDataAsSpan.Slice(I32Len, I64Len)));

                return new Counter(timestamp, count);
            }
            catch
            {
                return default;
            }
        }
    }

}
