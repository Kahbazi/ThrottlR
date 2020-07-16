using System;

namespace ThrottlR
{
    public static class ThrottlePolicyBuilderExtensions
    {
        public static ThrottlePolicyBuilder WithUsernameResolver(this ThrottlePolicyBuilder builder)
        {
            return builder.WithResolver(UsernameResolver.Instance);
        }

        public static ThrottlePolicyBuilder WithIpResolver(this ThrottlePolicyBuilder builder)
        {
            return builder.WithResolver(IpResolver.Instance);
        }

        public static ThrottlePolicyBuilder WithNoResolver(this ThrottlePolicyBuilder builder)
        {
            return builder.WithResolver(NoResolver.Instance);
        }

        public static ThrottlePolicyBuilder WithHostResolver(this ThrottlePolicyBuilder builder)
        {
            return builder.WithResolver(HostResolver.Instance);
        }

        public static ThrottlePolicyBuilder WithAccessTokenResolver(this ThrottlePolicyBuilder builder)
        {
            return builder.WithResolver(AccessTokenResolver.Instance);
        }

        public static ThrottlePolicyBuilder WithResolver<TResolver>(this ThrottlePolicyBuilder builder, IServiceProvider serviceProvider) where TResolver : IResolver
        {
            return builder.WithResolver(new TypeResolver<TResolver>(serviceProvider));
        }
    }
}
