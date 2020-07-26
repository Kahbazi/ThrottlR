using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace ThrottlR.Tests
{
    public class ThrottlerServiceTests
    {

        [Fact]
        public void ThrottlerService_GetRules_OnlyGeneralRules()
        {
            // Arrange
            var throttlerService = CreateThrottlerService();

            var generalRules = new List<ThrottleRule>
            {
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(8), Quota = 8 },
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(5), Quota = 5 },
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(2), Quota = 2 },
            };

            // Act
            var rules = throttlerService.GetRules(generalRules, Array.Empty<ThrottleRule>()).ToList();

            // Assert
            var rule1 = rules[0];
            Assert.NotNull(rule1);
            Assert.Equal(TimeSpan.FromSeconds(8), rule1.TimeWindow);
            Assert.Equal(8, rule1.Quota);

            var rule2 = rules[1];
            Assert.NotNull(rule2);
            Assert.Equal(TimeSpan.FromSeconds(5), rule2.TimeWindow);
            Assert.Equal(5, rule2.Quota);

            var rule3 = rules[2];
            Assert.NotNull(rule3);
            Assert.Equal(TimeSpan.FromSeconds(2), rule3.TimeWindow);
            Assert.Equal(2, rule3.Quota);
        }

        [Fact]
        public void ThrottlerService_GetRules_OnlySpeceficRules()
        {
            // Arrange
            var throttlerService = CreateThrottlerService();

            var specificRules = new List<ThrottleRule>
            {
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(8), Quota = 8 },
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(5), Quota = 5 },
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(2), Quota = 2 },
            };

            // Act
            var rules = throttlerService.GetRules(Array.Empty<ThrottleRule>(), specificRules).ToList();

            // Assert
            var rule1 = rules[0];
            Assert.NotNull(rule1);
            Assert.Equal(TimeSpan.FromSeconds(8), rule1.TimeWindow);
            Assert.Equal(8, rule1.Quota);

            var rule2 = rules[1];
            Assert.NotNull(rule2);
            Assert.Equal(TimeSpan.FromSeconds(5), rule2.TimeWindow);
            Assert.Equal(5, rule2.Quota);

            var rule3 = rules[2];
            Assert.NotNull(rule3);
            Assert.Equal(TimeSpan.FromSeconds(2), rule3.TimeWindow);
            Assert.Equal(2, rule3.Quota);
        }

        [Fact]
        public void ThrottlerService_GetRules_Returns_Specefic_Rules_When_Overlap_With_Genral_Rules()
        {
            // Arrange
            var throttlerService = CreateThrottlerService();

            var generalRules = new List<ThrottleRule>
            {
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(8), Quota = 88 },
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(5), Quota = 55 },
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(2), Quota = 22 },
            };

            var specificRules = new List<ThrottleRule>
            {
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(8), Quota = 8 },
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(5), Quota = 5 },
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(2), Quota = 2 },
            };


            // Act
            var rules = throttlerService.GetRules(generalRules, specificRules).ToList();

            // Assert
            var rule1 = rules[0];
            Assert.NotNull(rule1);
            Assert.Equal(TimeSpan.FromSeconds(8), rule1.TimeWindow);
            Assert.Equal(8, rule1.Quota);

            var rule2 = rules[1];
            Assert.NotNull(rule2);
            Assert.Equal(TimeSpan.FromSeconds(5), rule2.TimeWindow);
            Assert.Equal(5, rule2.Quota);

            var rule3 = rules[2];
            Assert.NotNull(rule3);
            Assert.Equal(TimeSpan.FromSeconds(2), rule3.TimeWindow);
            Assert.Equal(2, rule3.Quota);
        }

        [Fact]
        public void ThrottlerService_GetRules_Returns_Both_Specefic_And_Genral_Rules()
        {
            // Arrange
            var throttlerService = CreateThrottlerService();

            var generalRules = new List<ThrottleRule>
            {
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(9), Quota = 9 },
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(7), Quota = 7 },
            };

            var specificRules = new List<ThrottleRule>
            {
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(8), Quota = 8 },
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(6), Quota = 6 },
            };

            // Act
            var rules = throttlerService.GetRules(generalRules, specificRules).ToList();

            // Assert
            var rule1 = rules[0];
            Assert.NotNull(rule1);
            Assert.Equal(TimeSpan.FromSeconds(9), rule1.TimeWindow);
            Assert.Equal(9, rule1.Quota);

            var rule2 = rules[1];
            Assert.NotNull(rule2);
            Assert.Equal(TimeSpan.FromSeconds(8), rule2.TimeWindow);
            Assert.Equal(8, rule2.Quota);

            var rule3 = rules[2];
            Assert.NotNull(rule3);
            Assert.Equal(TimeSpan.FromSeconds(7), rule3.TimeWindow);
            Assert.Equal(7, rule3.Quota);

            var rule4 = rules[3];
            Assert.NotNull(rule4);
            Assert.Equal(TimeSpan.FromSeconds(6), rule4.TimeWindow);
            Assert.Equal(6, rule4.Quota);
        }

        [Fact]
        public void ThrottlerService_GetRules_Returns_More_Specefic_Than_Genral_Rules()
        {
            // Arrange
            var throttlerService = CreateThrottlerService();

            var generalRules = new List<ThrottleRule>
            {
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(9), Quota = 9 },
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(7), Quota = 7 },
            };

            var specificRules = new List<ThrottleRule>
            {
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(8), Quota = 8 },
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(6), Quota = 6 },
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(5), Quota = 5 },
            };

            // Act
            var rules = throttlerService.GetRules(generalRules, specificRules).ToList();

            // Assert
            var rule1 = rules[0];
            Assert.NotNull(rule1);
            Assert.Equal(TimeSpan.FromSeconds(9), rule1.TimeWindow);
            Assert.Equal(9, rule1.Quota);

            var rule2 = rules[1];
            Assert.NotNull(rule2);
            Assert.Equal(TimeSpan.FromSeconds(8), rule2.TimeWindow);
            Assert.Equal(8, rule2.Quota);

            var rule3 = rules[2];
            Assert.NotNull(rule3);
            Assert.Equal(TimeSpan.FromSeconds(7), rule3.TimeWindow);
            Assert.Equal(7, rule3.Quota);

            var rule4 = rules[3];
            Assert.NotNull(rule4);
            Assert.Equal(TimeSpan.FromSeconds(6), rule4.TimeWindow);
            Assert.Equal(6, rule4.Quota);

            var rule5 = rules[4];
            Assert.NotNull(rule5);
            Assert.Equal(TimeSpan.FromSeconds(5), rule5.TimeWindow);
            Assert.Equal(5, rule5.Quota);
        }

        [Fact]
        public void ThrottlerService_GetRules_Returns_More_Genral_Than_Specefic_Rules()
        {
            // Arrange
            var throttlerService = CreateThrottlerService();

            var generalRules = new List<ThrottleRule>
            {
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(9), Quota = 9 },
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(7), Quota = 7 },
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(5), Quota = 5 },
            };

            var specificRules = new List<ThrottleRule>
            {
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(8), Quota = 8 },
                new ThrottleRule { TimeWindow = TimeSpan.FromSeconds(6), Quota = 6 },
            };

            // Act
            var rules = throttlerService.GetRules(generalRules, specificRules).ToList();

            // Assert
            var rule1 = rules[0];
            Assert.NotNull(rule1);
            Assert.Equal(TimeSpan.FromSeconds(9), rule1.TimeWindow);
            Assert.Equal(9, rule1.Quota);

            var rule2 = rules[1];
            Assert.NotNull(rule2);
            Assert.Equal(TimeSpan.FromSeconds(8), rule2.TimeWindow);
            Assert.Equal(8, rule2.Quota);

            var rule3 = rules[2];
            Assert.NotNull(rule3);
            Assert.Equal(TimeSpan.FromSeconds(7), rule3.TimeWindow);
            Assert.Equal(7, rule3.Quota);

            var rule4 = rules[3];
            Assert.NotNull(rule4);
            Assert.Equal(TimeSpan.FromSeconds(6), rule4.TimeWindow);
            Assert.Equal(6, rule4.Quota);

            var rule5 = rules[4];
            Assert.NotNull(rule5);
            Assert.Equal(TimeSpan.FromSeconds(5), rule5.TimeWindow);
            Assert.Equal(5, rule5.Quota);
        }

        private static ThrottlerService CreateThrottlerService()
        {
            return new ThrottlerService(new TestCounterStore(), new TimeMachine());
        }
    }
}
