using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottlR.Tests
{
    public class TestCounterStore : ICounterStore
    {
        private readonly Dictionary<string, Counter> _cache = new Dictionary<string, Counter>();

        public ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken)
        {
            return new ValueTask<bool>(_cache.ContainsKey(key));
        }

        public ValueTask<Counter?> GetAsync(string key, CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(key, out var counter))
            {
                return new ValueTask<Counter?>(counter);
            }
            else
            {
                return new ValueTask<Counter?>(default(Counter?));
            }
        }

        public ValueTask RemoveAsync(string key, CancellationToken cancellationToken)
        {
            _cache.Remove(key);
            return new ValueTask();
        }

        public ValueTask SetAsync(string key, Counter counter, TimeSpan? expirationTime, CancellationToken cancellationToken)
        {
            _cache[key] = counter;
            return new ValueTask();
        }
    }
}
