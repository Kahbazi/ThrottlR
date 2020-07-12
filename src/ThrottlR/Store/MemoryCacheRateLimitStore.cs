using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottlR
{
    public class MemoryCacheRateLimitStore : IRateLimitStore
    {
        private readonly IMemoryCache _cache;

        public MemoryCacheRateLimitStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        public ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken)
        {
            return new ValueTask<bool>(_cache.TryGetValue(key, out _));
        }

        public ValueTask<RateLimitCounter?> GetAsync(string key, CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(key, out RateLimitCounter stored))
            {
                return new ValueTask<RateLimitCounter?>(stored);
            }

            return new ValueTask<RateLimitCounter?>(default(RateLimitCounter?));
        }

        public ValueTask RemoveAsync(string key, CancellationToken cancellationToken)
        {
            _cache.Remove(key);

            return new ValueTask();
        }

        public ValueTask SetAsync(string key, RateLimitCounter counter, TimeSpan? expirationTime, CancellationToken cancellationToken)
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
