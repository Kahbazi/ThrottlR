using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;

namespace Playground
{
    public static class Extensions
    {
        public static TBuilder RequireRateLimit<TBuilder>(this TBuilder builder, params string[] policies) where TBuilder : IEndpointConventionBuilder
        {
            foreach (var policy in policies)
            {
                builder.WithMetadata(new RateLimitPolicy(policy));
            }

            return builder;
        }

        public static TBuilder RequireRateLimit<TBuilder>(this TBuilder builder, PartitionedRateLimiter<HttpContext> rateLimiter) where TBuilder : IEndpointConventionBuilder
        {
            return builder.WithMetadata(new RateLimitInline(rateLimiter));
        }

        public static TBuilder RequireTokenBucketRateLimit<TBuilder>(this TBuilder builder, int tokenlimit, TimeSpan replenishmentPeriod, int tokensPerPeriod) where TBuilder : IEndpointConventionBuilder
        {
            return builder.WithMetadata(new TokenBucketRateLimitInline(tokenlimit, replenishmentPeriod, tokensPerPeriod));
        }

        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app, PartitionedOptions options)
        {
            return app.UseMiddleware<RateLimitMiddleware>(Options.Create<PartitionedOptions>(options));
        }
    }

}
