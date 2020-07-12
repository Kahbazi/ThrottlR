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

        public ValueTask<bool> ExistsAsync(string id, CancellationToken cancellationToken)
        {
            return new ValueTask<bool>(_cache.TryGetValue(id, out _));
        }

        public ValueTask<RateLimitCounter?> GetAsync(string id, CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(id, out RateLimitCounter stored))
            {
                return new ValueTask<RateLimitCounter?>(stored);
            }

            return new ValueTask<RateLimitCounter?>(default(RateLimitCounter));
        }

        public ValueTask RemoveAsync(string id, CancellationToken cancellationToken)
        {
            _cache.Remove(id);

            return new ValueTask();
        }

        public ValueTask SetAsync(string id, RateLimitCounter entry, TimeSpan? expirationTime, CancellationToken cancellationToken)
        {
            var options = new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove
            };

            if (expirationTime.HasValue)
            {
                options.SetAbsoluteExpiration(expirationTime.Value);
            }

            _cache.Set(id, entry, options);

            return new ValueTask();
        }
    }
}
