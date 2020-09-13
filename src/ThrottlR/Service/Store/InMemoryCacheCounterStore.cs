using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottlR
{
    public class InMemoryCacheCounterStore : ICounterStore
    {
        private readonly IMemoryCache _cache;

        public InMemoryCacheCounterStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        public ValueTask<Counter?> GetAsync(ThrottlerItem throttlerItem, CancellationToken cancellationToken)
        {
            var key = GenerateThrottlerItemKey(throttlerItem);
            if (_cache.TryGetValue(key, out Counter stored))
            {
                return new ValueTask<Counter?>(stored);
            }

            return new ValueTask<Counter?>(default(Counter?));
        }

        public ValueTask RemoveAsync(ThrottlerItem throttlerItem, CancellationToken cancellationToken)
        {
            var key = GenerateThrottlerItemKey(throttlerItem);
            _cache.Remove(key);

            return new ValueTask();
        }

        public ValueTask SetAsync(ThrottlerItem throttlerItem, Counter counter, TimeSpan? expirationTime, CancellationToken cancellationToken)
        {
            var options = new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove
            };

            if (expirationTime.HasValue)
            {
                options.SetAbsoluteExpiration(expirationTime.Value);
            }

            var key = GenerateThrottlerItemKey(throttlerItem);
            _cache.Set(key, counter, options);

            return new ValueTask();
        }

        public virtual string GenerateThrottlerItemKey(ThrottlerItem throttlerItem)
        {
            return throttlerItem.GenerateCounterKey("Throttler");
        }
    }
}
