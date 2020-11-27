using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ThrottlR.Service.Store;

namespace ThrottlR.Store
{
    public class InMemoryCacheCounterTests : CounterStoreTests
    {
        public override ICounterStore CreateCounterStore()
        {
            return new InMemoryCacheCounterStore(new MemoryCache(Options.Create(new MemoryCacheOptions())));
        }
    }
}
