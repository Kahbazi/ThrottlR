using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ThrottlR
{
    public abstract class RateLimitStoreTests
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

        [Fact]
        public async Task Set_Multiple_Get_Each()
        {
            var store = CreateRateLimitStore();

            var timestamp1 = DateTime.Now;
            var count1 = 11;
            await store.SetAsync("myKey1", new RateLimitCounter(timestamp1, count1), null, CancellationToken.None);

            var timestamp2 = DateTime.Now;
            var count2 = 12;
            await store.SetAsync("myKey2", new RateLimitCounter(timestamp2, count2), null, CancellationToken.None);

            var counter1 = await store.GetAsync("myKey1", CancellationToken.None);
            var counter2 = await store.GetAsync("myKey2", CancellationToken.None);

            Assert.True(counter1.HasValue);
            Assert.Equal(count1, counter1.Value.Count);
            Assert.Equal(timestamp1, counter1.Value.Timestamp);

            Assert.True(counter2.HasValue);
            Assert.Equal(count2, counter2.Value.Count);
            Assert.Equal(timestamp2, counter2.Value.Timestamp);
        }

        [Fact]
        public async Task Set_Multiple_Remove_One()
        {
            var store = CreateRateLimitStore();

            var timestamp1 = DateTime.Now;
            var count1 = 11;
            await store.SetAsync("myKey1", new RateLimitCounter(timestamp1, count1), null, CancellationToken.None);

            var timestamp2 = DateTime.Now;
            var count2 = 12;
            await store.SetAsync("myKey2", new RateLimitCounter(timestamp2, count2), null, CancellationToken.None);

            await store.RemoveAsync("myKey1", CancellationToken.None);
            
            var counter1 = await store.GetAsync("myKey1", CancellationToken.None);
            var counter2 = await store.GetAsync("myKey2", CancellationToken.None);

            Assert.False(counter1.HasValue);

            Assert.True(counter2.HasValue);
            Assert.Equal(count2, counter2.Value.Count);
            Assert.Equal(timestamp2, counter2.Value.Timestamp);
        }

        public abstract IRateLimitStore CreateRateLimitStore();
    }

}
