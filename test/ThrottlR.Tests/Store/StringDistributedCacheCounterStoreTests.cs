using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ThrottlR.Service.Store;

namespace ThrottlR.Store
{
    public class StringDistributedCacheCounterStoreTests : CounterStoreTests
    {
        public override ICounterStore CreateCounterStore()
        {
            return new StringDistributedCacheCounterStore(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())));
        }
    }
}
