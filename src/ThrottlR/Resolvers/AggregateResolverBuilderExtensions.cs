using System;

namespace ThrottlR
{
    public static class AggregateResolverBuilderExtensions
    {
        public static AggregateResolverBuilder AppendUsernameResolver(this AggregateResolverBuilder builder)
        {
            return builder.AppendResolver(UsernameResolver.Instance);
        }

        public static AggregateResolverBuilder AppendIpResolver(this AggregateResolverBuilder builder)
        {
            return builder.AppendResolver(IpResolver.Instance);
        }

        public static AggregateResolverBuilder AppendNoResolver(this AggregateResolverBuilder builder)
        {
            return builder.AppendResolver(NoResolver.Instance);
        }

        public static AggregateResolverBuilder AppendHostResolver(this AggregateResolverBuilder builder)
        {
            return builder.AppendResolver(HostResolver.Instance);
        }

        public static AggregateResolverBuilder AppendResolver<TResolver>(this AggregateResolverBuilder builder, IServiceProvider serviceProvider) where TResolver : IResolver
        {
            return builder.AppendResolver(new TypeResolver<TResolver>(serviceProvider));
        }
    }
}
