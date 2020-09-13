using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottlR.Tests
{
    public class TestCounterStore : ICounterStore
    {
        private readonly Dictionary<string, Counter> _cache = new Dictionary<string, Counter>();

        public ValueTask<Counter?> GetAsync(ThrottlerItem throttlerItem, CancellationToken cancellationToken)
        {
            var key = throttlerItem.GenerateCounterKey();
            if (_cache.TryGetValue(key, out var counter))
            {
                return new ValueTask<Counter?>(counter);
            }
            else
            {
                return new ValueTask<Counter?>(default(Counter?));
            }
        }

        public ValueTask RemoveAsync(ThrottlerItem throttlerItem, CancellationToken cancellationToken)
        {
            var key = throttlerItem.GenerateCounterKey();
            _cache.Remove(key);
            return new ValueTask();
        }

        public ValueTask SetAsync(ThrottlerItem throttlerItem, Counter counter, TimeSpan? expirationTime, CancellationToken cancellationToken)
        {
            var key = throttlerItem.GenerateCounterKey();
            _cache[key] = counter;
            return new ValueTask();
        }
    }
}
