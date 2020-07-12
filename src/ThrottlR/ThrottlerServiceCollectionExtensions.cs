using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThrottlR;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ThrottlerServiceCollectionExtensions
    {
        public static IServiceCollection AddThrottlR(this IServiceCollection services, Action<ThrottleOptions> configure)
        {
            services.TryAddSingleton<ThrottlerService, RateLimitProcessor>();
            services.TryAddSingleton<IThrottlePolicyProvider, DefaultThrottlePolicyProvider>();
            services.TryAddSingleton<ISystemClock, SystemClock>();

            services.AddOptions<ThrottleOptions>().Configure(configure);

            return services;
        }
    }
}
