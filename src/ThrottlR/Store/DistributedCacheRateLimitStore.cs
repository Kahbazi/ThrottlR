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

        public async ValueTask SetAsync(string id, RateLimitCounter entry, TimeSpan? expirationTime, CancellationToken cancellationToken)
        {
            var options = new DistributedCacheEntryOptions();

            if (expirationTime.HasValue)
            {
                options.SetAbsoluteExpiration(expirationTime.Value);
            }

            await _cache.SetStringAsync(id, JsonSerializer.Serialize(entry), options, cancellationToken);
        }

        public async ValueTask<bool> ExistsAsync(string id, CancellationToken cancellationToken)
        {
            var stored = await _cache.GetStringAsync(id, cancellationToken);

            return !string.IsNullOrEmpty(stored);
        }

        public async ValueTask<RateLimitCounter?> GetAsync(string id, CancellationToken cancellationToken)
        {
            var stored = await _cache.GetStringAsync(id, cancellationToken);

            if (!string.IsNullOrEmpty(stored))
            {
                return JsonSerializer.Deserialize<RateLimitCounter>(stored);
            }

            return default;
        }

        public async ValueTask RemoveAsync(string id, CancellationToken cancellationToken)
        {
            await _cache.RemoveAsync(id, cancellationToken);
        }
    }
}
