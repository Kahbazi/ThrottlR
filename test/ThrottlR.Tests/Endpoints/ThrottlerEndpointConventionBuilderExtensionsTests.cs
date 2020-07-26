using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace ThrottlR.Endpoints
{
    public class ThrottlerEndpointConventionBuilderExtensionsTests
    {
        [Fact]
        public void EndpointConventionBuilderExtensions_Throttle()
        {
            // Arrange
            var conventionBuilder = new TestEndpointConventionBuilder();

            // Act
            conventionBuilder.Throttle();

            var endpointBuilder = new TestEndpointBuilder();

            for (var i = 0; i < conventionBuilder.Actions.Count; i++)
            {
                conventionBuilder.Actions[i](endpointBuilder);
            }

            var endpoint = endpointBuilder.Build();

            var throttleMetadata = endpoint.Metadata.GetMetadata<IThrottleMetadata>();
            Assert.NotNull(throttleMetadata);
            Assert.Null(throttleMetadata.PolicyName);
        }

        [Fact]
        public void EndpointConventionBuilderExtensions_Throttle_Policy()
        {
            // Arrange
            var conventionBuilder = new TestEndpointConventionBuilder();

            // Act
            conventionBuilder.Throttle("policy-1");

            var endpointBuilder = new TestEndpointBuilder();

            for (var i = 0; i < conventionBuilder.Actions.Count; i++)
            {
                conventionBuilder.Actions[i](endpointBuilder);
            }

            var endpoint = endpointBuilder.Build();

            var throttleMetadata = endpoint.Metadata.GetMetadata<IThrottleMetadata>();
            Assert.NotNull(throttleMetadata);
            Assert.Equal("policy-1", throttleMetadata.PolicyName);
        }

        [Theory]
        [InlineData(null, null, null, null)]
        [InlineData(null, null, null, 5)]
        [InlineData(null, null, 5, null)]
        [InlineData(null, null, 5, 5)]
        [InlineData(null, 5, null, null)]
        [InlineData(null, 5, null, 5)]
        [InlineData(null, 5, 5, null)]
        [InlineData(null, 5, 5, 5)]
        [InlineData(5, null, null, null)]
        [InlineData(5, null, null, 5)]
        [InlineData(5, null, 5, null)]
        [InlineData(5, null, 5, 5)]
        [InlineData(5, 5, null, null)]
        [InlineData(5, 5, null, 5)]
        [InlineData(5, 5, 5, null)]
        [InlineData(5, 5, 5, 5)]
        public void EndpointConventionBuilderExtensions_Throttle_Rule(long? perSecond, long? perMinute, long? perHour, long? perDay)
        {
            // Arrange
            var conventionBuilder = new TestEndpointConventionBuilder();

            // Act
            conventionBuilder.Throttle(perSecond, perMinute, perHour, perDay);

            var endpointBuilder = new TestEndpointBuilder();

            for (var i = 0; i < conventionBuilder.Actions.Count; i++)
            {
                conventionBuilder.Actions[i](endpointBuilder);
            }

            var endpoint = endpointBuilder.Build();

            var throttleRulesMetadata = endpoint.Metadata.GetMetadata<IThrottleRulesMetadata>();
            Assert.NotNull(throttleRulesMetadata);
            Assert.Null(throttleRulesMetadata.PolicyName);

            var rules = throttleRulesMetadata.GeneralRules;
            Assert.NotNull(rules);

            if (perSecond.HasValue)
            {
                var rule = Assert.Single(rules.Where(x => x.TimeWindow == TimeSpan.FromSeconds(1)));
                Assert.Equal(perSecond.Value, rule.Quota);
            }

            if (perMinute.HasValue)
            {
                var rule = Assert.Single(rules.Where(x => x.TimeWindow == TimeSpan.FromMinutes(1)));
                Assert.Equal(perMinute.Value, rule.Quota);
            }

            if (perHour.HasValue)
            {
                var rule = Assert.Single(rules.Where(x => x.TimeWindow == TimeSpan.FromHours(1)));
                Assert.Equal(perHour.Value, rule.Quota);
            }

            if (perDay.HasValue)
            {
                var rule = Assert.Single(rules.Where(x => x.TimeWindow == TimeSpan.FromDays(1)));
                Assert.Equal(perDay.Value, rule.Quota);
            }

            var rulesCount = (perSecond.HasValue ? 1 : 0)
               + (perMinute.HasValue ? 1 : 0)
               + (perHour.HasValue ? 1 : 0)
               + (perDay.HasValue ? 1 : 0);

            Assert.Equal(rulesCount, rules.Count);
        }
    }

    public class TestEndpointConventionBuilder : IEndpointConventionBuilder
    {
        public List<Action<EndpointBuilder>> Actions { get; } = new List<Action<EndpointBuilder>>();

        public void Add(Action<EndpointBuilder> convention)
        {
            Actions.Add(convention);
        }
    }

    public class TestEndpointBuilder : EndpointBuilder
    {
        public override Endpoint Build()
        {
            return new Endpoint(RequestDelegate, new EndpointMetadataCollection(Metadata), DisplayName);
        }
    }
}
