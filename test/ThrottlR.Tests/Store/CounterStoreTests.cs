using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ThrottlR
{
    public abstract class CounterStoreTests
    {
        [Fact]
        public async Task Get()
        {
            var store = CreateCounterStore();

            var timestamp = DateTime.Now;

            var throttleRule = new ThrottleRule { Quota = 10, TimeWindow = TimeSpan.FromSeconds(10) };
            var throttlerItem = new ThrottlerItem(throttleRule, "policy", "scope", "endpoint");

            await store.SetAsync(throttlerItem, new Counter(timestamp, 12), null, CancellationToken.None);

            var counter = await store.GetAsync(throttlerItem, CancellationToken.None);

            Assert.True(counter.HasValue);
            Assert.Equal(12, counter.Value.Count);
            Assert.Equal(timestamp, counter.Value.Timestamp);
        }

        [Fact]
        public async Task Get_Null()
        {
            var store = CreateCounterStore();

            var throttleRule = new ThrottleRule { Quota = 10, TimeWindow = TimeSpan.FromSeconds(10) };
            var throttlerItem = new ThrottlerItem(throttleRule, "policy", "scope", "endpoint");

            var counter = await store.GetAsync(throttlerItem, CancellationToken.None);

            Assert.False(counter.HasValue);
        }

        [Fact]
        public async Task Remove()
        {
            var store = CreateCounterStore();

            var timestamp = DateTime.Now;

            var throttleRule = new ThrottleRule { Quota = 10, TimeWindow = TimeSpan.FromSeconds(10) };
            var throttlerItem = new ThrottlerItem(throttleRule, "policy", "scope", "endpoint");

            await store.SetAsync(throttlerItem, new Counter(timestamp, 12), null, CancellationToken.None);

            await store.RemoveAsync(throttlerItem, CancellationToken.None);

            var counter = await store.GetAsync(throttlerItem, CancellationToken.None);

            Assert.False(counter.HasValue);
        }

        [Fact]
        public async Task Set_Multiple_Get_Each()
        {
            var store = CreateCounterStore();

            var timestamp1 = DateTime.Now;
            var count1 = 11;

            var throttleRule = new ThrottleRule { Quota = 10, TimeWindow = TimeSpan.FromSeconds(10) };
            var throttlerItem1 = new ThrottlerItem(throttleRule, "policy", "scope1", "endpoint");
            var throttlerItem2 = new ThrottlerItem(throttleRule, "policy", "scope2", "endpoint");

            await store.SetAsync(throttlerItem1, new Counter(timestamp1, count1), null, CancellationToken.None);

            var timestamp2 = DateTime.Now;
            var count2 = 12;
            await store.SetAsync(throttlerItem2, new Counter(timestamp2, count2), null, CancellationToken.None);

            var counter1 = await store.GetAsync(throttlerItem1, CancellationToken.None);
            var counter2 = await store.GetAsync(throttlerItem2, CancellationToken.None);

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
            var store = CreateCounterStore();

            var timestamp1 = DateTime.Now;
            var count1 = 11;

            var throttleRule = new ThrottleRule { Quota = 10, TimeWindow = TimeSpan.FromSeconds(10) };
            var throttlerItem1 = new ThrottlerItem(throttleRule, "policy", "scope1", "endpoint");
            var throttlerItem2 = new ThrottlerItem(throttleRule, "policy", "scope2", "endpoint");

            await store.SetAsync(throttlerItem1, new Counter(timestamp1, count1), null, CancellationToken.None);

            var timestamp2 = DateTime.Now;
            var count2 = 12;
            await store.SetAsync(throttlerItem2, new Counter(timestamp2, count2), null, CancellationToken.None);

            await store.RemoveAsync(throttlerItem1, CancellationToken.None);

            var counter1 = await store.GetAsync(throttlerItem1, CancellationToken.None);
            var counter2 = await store.GetAsync(throttlerItem2, CancellationToken.None);

            Assert.False(counter1.HasValue);

            Assert.True(counter2.HasValue);
            Assert.Equal(count2, counter2.Value.Count);
            Assert.Equal(timestamp2, counter2.Value.Timestamp);
        }

        public abstract ICounterStore CreateCounterStore();
    }

}
