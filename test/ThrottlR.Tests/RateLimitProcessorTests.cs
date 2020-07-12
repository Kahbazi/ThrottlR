using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Xunit;
using System.Threading;

namespace ThrottlR.Tests
{
    public class RateLimitProcessorTests
    {
        [Fact]
        public void IsSafe_Safe()
        {
            // Arrange
            var (processor, timeMachine) = CreateProcessor();

            var identity = "John";
            var safeList = new[] { "John", "Joe", "Jack" };

            // Act
            var result = processor.IsSafe(identity, safeList);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsSafe_NotSafe()
        {
            // Arrange
            var (processor, timeMachine) = CreateProcessor();

            var identity = "John";
            var safeList = new[] { "Joe", "Jack" };

            // Act
            var result = processor.IsSafe(identity, safeList);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsSafe_NullList_NotSafe()
        {
            // Arrange
            var (processor, timeMachine) = CreateProcessor();

            var identity = "John";

            // Act
            var result = processor.IsSafe(identity, null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetRateLimitHeaders()
        {
            // Arrange
            var (processor, store) = CreateProcessor();
            var rule = new ThrottleRule { Limit = 5, Period = TimeSpan.FromSeconds(1) };
            var counter = new RateLimitCounter(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), 3);

            // Act
            var headers = processor.GetRateLimitHeaders(counter, rule);


            Assert.Equal("00:00:01", headers.Limit);
            Assert.Equal("2", headers.Remaining);
            Assert.Equal("2020-01-01T00:00:01.0000000+00", headers.Reset);
        }

        [Fact]
        public async Task ProcessRequest_FirstTime()
        {
            // Arrange
            var (processor, timeMachine) = CreateProcessor();

            var rule = new ThrottleRule { Limit = 5, Period = TimeSpan.FromSeconds(1) };

            // Act
            var headers = processor.ProcessRequestAsync("counter-1", rule, CancellationToken.None);


            
        }

        private static (RateLimitProcessor processor, TimeMachine timeMachine) CreateProcessor()
        {
            var options = Options.Create(new ThrottleOptions());
            var store = new TestRateLimitStore();
            var clock = new TimeMachine();
            return (new RateLimitProcessor(options, store, clock), clock);
        }
    }
}
