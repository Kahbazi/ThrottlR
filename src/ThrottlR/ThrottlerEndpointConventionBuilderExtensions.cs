using ThrottlR;

namespace Microsoft.AspNetCore.Builder
{
    public static class ThrottlerEndpointConventionBuilderExtensions
    {
        public static TBuilder Throttle<TBuilder>(this TBuilder builder, string policy) where TBuilder : IEndpointConventionBuilder
        {
            builder.Add(endpointBuilder =>
            {
                endpointBuilder.Metadata.Add(new ThrottleMetadata(policy));
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
                endpointBuilder.Metadata.Add(new ThrottleMetadata());
            });
            return builder;
        }
    }
}
