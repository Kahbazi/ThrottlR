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

        public ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken)
        {
            return new ValueTask<bool>(_cache.TryGetValue(key, out _));
        }

        public ValueTask<Counter?> GetAsync(string key, CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(key, out Counter stored))
            {
                return new ValueTask<Counter?>(stored);
            }

            return new ValueTask<Counter?>(default(Counter?));
        }

        public ValueTask RemoveAsync(string key, CancellationToken cancellationToken)
        {
            _cache.Remove(key);

            return new ValueTask();
        }

        public ValueTask SetAsync(string key, Counter counter, TimeSpan? expirationTime, CancellationToken cancellationToken)
        {
            var options = new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove
            };

            if (expirationTime.HasValue)
            {
                options.SetAbsoluteExpiration(expirationTime.Value);
            }

            _cache.Set(key, counter, options);

            return new ValueTask();
        }
    }
}
