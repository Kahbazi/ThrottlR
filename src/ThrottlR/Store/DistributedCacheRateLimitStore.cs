using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottlR
{
    public class DistributedCacheRateLimitStore : IRateLimitStore
    {
        private readonly IDistributedCache _cache;

        public DistributedCacheRateLimitStore(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async ValueTask SetAsync(string key, RateLimitCounter counter, TimeSpan? expirationTime, CancellationToken cancellationToken)
        {
            var options = new DistributedCacheEntryOptions();

            if (expirationTime.HasValue)
            {
                options.SetAbsoluteExpiration(expirationTime.Value);
            }

            await _cache.SetStringAsync(key, JsonSerializer.Serialize(counter), options, cancellationToken);
        }

        public async ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken)
        {
            var stored = await _cache.GetStringAsync(key, cancellationToken);

            return !string.IsNullOrEmpty(stored);
        }

        public async ValueTask<RateLimitCounter?> GetAsync(string key, CancellationToken cancellationToken)
        {
            var stored = await _cache.GetStringAsync(key, cancellationToken);

            if (!string.IsNullOrEmpty(stored))
            {
                return JsonSerializer.Deserialize<RateLimitCounter>(stored);
            }

            return default;
        }

        public async ValueTask RemoveAsync(string key, CancellationToken cancellationToken)
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
    }
}
