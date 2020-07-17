using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThrottlR;
using ThrottlR.Service;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ThrottlerServiceCollectionExtensions
    {
        public static IThrottlerBuilder AddThrottlR(this IServiceCollection services, Action<ThrottleOptions> configure)
        {
            var builder = new ThrottlerBuilder(services);

            services.TryAddSingleton<IThrottlerService, ThrottlerService>();
            services.TryAddSingleton<IThrottlePolicyProvider, DefaultThrottlePolicyProvider>();
            services.TryAddSingleton<ISystemClock, SystemClock>();
            services.TryAddSingleton<ICounterKeyBuilder, DefaultCounterKeyBuilder>();

            services.AddOptions<ThrottleOptions>().Configure(configure);

            services.AddMemoryCache();

            return builder;
        }

        public static IThrottlerBuilder AddInMemoryRateLimitStore(this IThrottlerBuilder builder)
        {
            builder.Services.TryAddSingleton<IRateLimitStore, MemoryCacheRateLimitStore>();

            return builder;
        }

        public static IThrottlerBuilder AddDistributedRateLimitStore(this IThrottlerBuilder builder)
        {
            builder.Services.TryAddSingleton<IRateLimitStore, DistributedCacheRateLimitStore>();

            return builder;
        }
    }
}
