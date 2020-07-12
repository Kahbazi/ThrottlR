using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace ThrottlR
{
    public class MemoryCacheRateLimitStoreTests : CacheRateLimitStoreTests
    {
        public override IRateLimitStore CreateRateLimitStore()
        {
            return new MemoryCacheRateLimitStore(new MemoryCache(Options.Create(new MemoryCacheOptions())));
        }
    }

    public class DistributedCacheRateLimitStoreTests : CacheRateLimitStoreTests
    {
        public override IRateLimitStore CreateRateLimitStore()
        {
            return new DistributedCacheRateLimitStore(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())));
        }
    }

    public abstract class CacheRateLimitStoreTests
    {
        [Fact]
        public async Task Get()
        {
            var store = CreateRateLimitStore();

            var timestamp = DateTime.Now;
            await store.SetAsync("myKey", new RateLimitCounter(timestamp, 12), null, CancellationToken.None);


            var counter = await store.GetAsync("myKey", CancellationToken.None);

            Assert.True(counter.HasValue);
            Assert.Equal(12, counter.Value.Count);
            Assert.Equal(timestamp, counter.Value.Timestamp);
        }

        [Fact]
        public async Task Get_Null()
        {
            var store = CreateRateLimitStore();

            var counter = await store.GetAsync("myKey", CancellationToken.None);

            Assert.False(counter.HasValue);
        }

        [Fact]
        public async Task Exists_True()
        {
            var store = CreateRateLimitStore();

            var timestamp = DateTime.Now;
            await store.SetAsync("myKey", new RateLimitCounter(timestamp, 12), null, CancellationToken.None);


            var result = await store.ExistsAsync("myKey", CancellationToken.None);

            Assert.True(result);
        }

        [Fact]
        public async Task Exists_False()
        {
            var store = CreateRateLimitStore();

            var timestamp = DateTime.Now;
            await store.SetAsync("myKey", new RateLimitCounter(timestamp, 12), null, CancellationToken.None);


            var result = await store.ExistsAsync("anotherKey", CancellationToken.None);

            Assert.False(result);
        }

        [Fact]
        public async Task Remove()
        {
            var store = CreateRateLimitStore();

            var timestamp = DateTime.Now;
            await store.SetAsync("myKey", new RateLimitCounter(timestamp, 12), null, CancellationToken.None);

            var result = await store.ExistsAsync("myKey", CancellationToken.None);

            Assert.True(result);

            await store.RemoveAsync("myKey", CancellationToken.None);

            var counter = await store.GetAsync("myKey", CancellationToken.None);
            result = await store.ExistsAsync("myKey", CancellationToken.None);

            Assert.False(counter.HasValue);
            Assert.False(result);
        }

        public abstract IRateLimitStore CreateRateLimitStore();
    }
}
