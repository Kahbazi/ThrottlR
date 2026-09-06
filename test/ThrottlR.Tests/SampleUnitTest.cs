using System;
using Xunit;

namespace ThrottlR.Tests
{
    public class SampleUnitTest
    {
        [Fact]
        public void ThrottleRule_Can_Be_Created_With_TimeWindow_And_Quota()
        {
            // Arrange
            var timeWindow = TimeSpan.FromSeconds(10);
            const int quota = 3;

            // Act
            var rule = new ThrottleRule
            {
                TimeWindow = timeWindow,
                Quota = quota
            };

            // Assert
            Assert.Equal(timeWindow, rule.TimeWindow);
            Assert.Equal(quota, rule.Quota);
        }
    }
}
