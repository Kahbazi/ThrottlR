using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ThrottlR
{
    public class DistributedCacheCounterStoreTests : CounterStoreTests
    {
        public override ICounterStore CreateCounterStore()
        {
            return new DistributedCacheCounterStore(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())));
        }
    }
}
