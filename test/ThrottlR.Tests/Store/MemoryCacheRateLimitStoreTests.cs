using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ThrottlR
{
    public class MemoryCacheRateLimitStoreTests : RateLimitStoreTests
    {
        public override IRateLimitStore CreateRateLimitStore()
        {
            return new MemoryCacheRateLimitStore(new MemoryCache(Options.Create(new MemoryCacheOptions())));
        }
    }
}
