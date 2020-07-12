using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottlR.Tests
{
    public class TestRateLimitStore : IRateLimitStore
    {
        private readonly Dictionary<string, RateLimitCounter> _cache = new Dictionary<string, RateLimitCounter>();

        public ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken)
        {
            return new ValueTask<bool>(_cache.ContainsKey(key));
        }

        public ValueTask<RateLimitCounter?> GetAsync(string key, CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(key, out var counter))
            {
                return new ValueTask<RateLimitCounter?>(counter);
            }
            else
            {
                return new ValueTask<RateLimitCounter?>(default(RateLimitCounter?));
            }
        }

        public ValueTask RemoveAsync(string key, CancellationToken cancellationToken)
        {
            _cache.Remove(key);
            return new ValueTask();
        }

        public ValueTask SetAsync(string key, RateLimitCounter counter, TimeSpan? expirationTime, CancellationToken cancellationToken)
        {
            _cache[key] = counter;
            return new ValueTask();
        }
    }
}
