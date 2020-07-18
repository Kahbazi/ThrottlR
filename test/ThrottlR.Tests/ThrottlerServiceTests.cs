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

            var policy = new ThrottlePolicyBuilder()
                .WithGeneralRule(TimeSpan.FromSeconds(8), 8)
                .WithGeneralRule(TimeSpan.FromSeconds(5), 5)
                .WithGeneralRule(TimeSpan.FromSeconds(2), 2)
                .Build();

            // Act
            var rules = throttlerService.GetRules(policy, "identity").ToList();

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

            var policy = new ThrottlePolicyBuilder()
                .WithSpecificRule("identity", TimeSpan.FromSeconds(8), 8)
                .WithSpecificRule("identity", TimeSpan.FromSeconds(5), 5)
                .WithSpecificRule("identity", TimeSpan.FromSeconds(2), 2)
                .Build();

            // Act
            var rules = throttlerService.GetRules(policy, "identity").ToList();

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

            var policy = new ThrottlePolicyBuilder()
                .WithGeneralRule(TimeSpan.FromSeconds(8), 88)
                .WithGeneralRule(TimeSpan.FromSeconds(5), 55)
                .WithGeneralRule(TimeSpan.FromSeconds(2), 22)
                .WithSpecificRule("identity", TimeSpan.FromSeconds(8), 8)
                .WithSpecificRule("identity", TimeSpan.FromSeconds(5), 5)
                .WithSpecificRule("identity", TimeSpan.FromSeconds(2), 2)
                .Build();

            // Act
            var rules = throttlerService.GetRules(policy, "identity").ToList();

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

            var policy = new ThrottlePolicyBuilder()
                .WithGeneralRule(TimeSpan.FromSeconds(9), 9)
                .WithGeneralRule(TimeSpan.FromSeconds(7), 7)
                .WithSpecificRule("identity", TimeSpan.FromSeconds(8), 8)
                .WithSpecificRule("identity", TimeSpan.FromSeconds(6), 6)
                .Build();

            // Act
            var rules = throttlerService.GetRules(policy, "identity").ToList();

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

            var policy = new ThrottlePolicyBuilder()
                .WithGeneralRule(TimeSpan.FromSeconds(9), 9)
                .WithGeneralRule(TimeSpan.FromSeconds(7), 7)
                .WithSpecificRule("identity", TimeSpan.FromSeconds(8), 8)
                .WithSpecificRule("identity", TimeSpan.FromSeconds(6), 6)
                .WithSpecificRule("identity", TimeSpan.FromSeconds(5), 5)
                .Build();

            // Act
            var rules = throttlerService.GetRules(policy, "identity").ToList();

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

            var policy = new ThrottlePolicyBuilder()
                .WithGeneralRule(TimeSpan.FromSeconds(9), 9)
                .WithGeneralRule(TimeSpan.FromSeconds(7), 7)
                .WithGeneralRule(TimeSpan.FromSeconds(5), 5)
                .WithSpecificRule("identity", TimeSpan.FromSeconds(8), 8)
                .WithSpecificRule("identity", TimeSpan.FromSeconds(6), 6)
                .Build();

            // Act
            var rules = throttlerService.GetRules(policy, "identity").ToList();

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
