using System;
using System.Linq;
using Xunit;

namespace ThrottlR
{
    public class ThrottleAttributeTests
    {
        [Fact]
        public void ThrottleAttribute_Constructor_Order()
        {
            // Arrange
            var throttleAttribute = new ThrottleAttribute
            {
                PerSecond = 1,
                PerMinute = 10,
                PerHour = 100,
                PerDay = 1000
            };

            // Act
            IThrottleRulesMetadata throttleRules = throttleAttribute;
            var rules = throttleRules.GeneralRules;

            // Assert
            Assert.Equal(4, rules.Count);

            var rule1 = rules[0];
            Assert.Equal(1000, rule1.Quota);
            Assert.Equal(TimeSpan.FromDays(1), rule1.TimeWindow);

            var rule2 = rules[1];
            Assert.Equal(100, rule2.Quota);
            Assert.Equal(TimeSpan.FromHours(1), rule2.TimeWindow);

            var rule3 = rules[2];
            Assert.Equal(10, rule3.Quota);
            Assert.Equal(TimeSpan.FromMinutes(1), rule3.TimeWindow);

            var rule4 = rules[3];
            Assert.Equal(1, rule4.Quota);
            Assert.Equal(TimeSpan.FromSeconds(1), rule4.TimeWindow);
        }


        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(0, 0, 0, 1)]
        [InlineData(0, 0, 2, 0)]
        [InlineData(0, 0, 2, 1)]
        [InlineData(0, 3, 0, 0)]
        [InlineData(0, 3, 0, 1)]
        [InlineData(0, 3, 2, 0)]
        [InlineData(0, 3, 2, 1)]
        [InlineData(4, 0, 0, 0)]
        [InlineData(4, 0, 0, 1)]
        [InlineData(4, 0, 2, 0)]
        [InlineData(4, 0, 2, 1)]
        [InlineData(4, 3, 0, 0)]
        [InlineData(4, 3, 0, 1)]
        [InlineData(4, 3, 2, 0)]
        [InlineData(4, 3, 2, 1)]
        public void ThrottleAttribute_Constructor_Rules(long perSecond, long perMinute, long perHour, long perDay)
        {
            // Arrange
            var throttleAttribute = new ThrottleAttribute
            {
                PerSecond = perSecond,
                PerMinute = perMinute,
                PerHour = perHour,
                PerDay = perDay
            };

            // Act
            IThrottleRulesMetadata throttleRules = throttleAttribute;
            var rules = throttleRules.GeneralRules;

            // Assert
            Assert.NotNull(rules);

            if (perSecond > 0)
            {
                var rule = Assert.Single(rules.Where(x => x.TimeWindow == TimeSpan.FromSeconds(1)));
                Assert.Equal(perSecond, rule.Quota);
            }

            if (perMinute > 0)
            {
                var rule = Assert.Single(rules.Where(x => x.TimeWindow == TimeSpan.FromMinutes(1)));
                Assert.Equal(perMinute, rule.Quota);
            }

            if (perHour > 0)
            {
                var rule = Assert.Single(rules.Where(x => x.TimeWindow == TimeSpan.FromHours(1)));
                Assert.Equal(perHour, rule.Quota);
            }

            if (perDay > 0)
            {
                var rule = Assert.Single(rules.Where(x => x.TimeWindow == TimeSpan.FromDays(1)));
                Assert.Equal(perDay, rule.Quota);
            }

            var rulesCount = (perSecond > 0 ? 1 : 0)
                           + (perMinute > 0 ? 1 : 0)
                           + (perHour > 0 ? 1 : 0)
                           + (perDay > 0 ? 1 : 0);

            Assert.Equal(rulesCount, rules.Count);
        }
    }
}
