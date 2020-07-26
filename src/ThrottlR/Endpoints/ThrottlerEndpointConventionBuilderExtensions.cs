using ThrottlR;

namespace Microsoft.AspNetCore.Builder
{
    public static class ThrottlerEndpointConventionBuilderExtensions
    {
        private static readonly EnableThrottle _throttleMetadata = new EnableThrottle();

        public static TBuilder Throttle<TBuilder>(this TBuilder builder, string policy) where TBuilder : IEndpointConventionBuilder
        {
            builder.Add(endpointBuilder =>
            {
                endpointBuilder.Metadata.Add(new EnableThrottle(policy));
            });
            return builder;
        }

        /// <summary>
        /// The default policy would be applied
        /// </summary>
        /// <typeparam name="TBuilder"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static TBuilder Throttle<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        {
            builder.Add(endpointBuilder =>
            {
                endpointBuilder.Metadata.Add(_throttleMetadata);
            });
            return builder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TBuilder"></typeparam>
        /// <param name="builder"></param>
        /// <param name="perSecond"></param>
        /// <param name="perMinute"></param>
        /// <param name="perHour"></param>
        /// <param name="perDay"></param>
        /// <param name="policyName"></param>
        /// <returns></returns>
        public static TBuilder Throttle<TBuilder>(this TBuilder builder, long? perSecond = null, long? perMinute = null, long? perHour = null, long? perDay = null, string policyName = null) where TBuilder : IEndpointConventionBuilder
        {
            builder.Add(endpointBuilder =>
            {
                var throttleAttribute = new ThrottleAttribute
                {
                    PerSecond = perSecond ?? 0,
                    PerMinute = perMinute ?? 0,
                    PerHour = perHour ?? 0,
                    PerDay = perDay ?? 0,
                    PolicyName = policyName
                };

                endpointBuilder.Metadata.Add(throttleAttribute);
            });
            return builder;
        }
    }
}
