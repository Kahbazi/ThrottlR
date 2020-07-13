using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ThrottlR
{
    public class DistributedCacheRateLimitStoreTests : RateLimitStoreTests
    {
        public override IRateLimitStore CreateRateLimitStore()
        {
            return new DistributedCacheRateLimitStore(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())));
        }
    }
}
