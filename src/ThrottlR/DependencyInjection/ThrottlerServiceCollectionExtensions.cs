using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThrottlR;

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

            services.AddOptions<ThrottleOptions>()
                .Configure(configure)
                .PostConfigure(options =>
                {
                    foreach (var policy in options.PolicyMap.Select(kvp => kvp.Value.policy))
                    {
                        foreach (var kvp in policy.SafeList)
                        {
                            policy.SafeList[kvp.Key] = kvp.Value
                                                          .Distinct()
                                                          .ToList();
                        }
                        

                        policy.GeneralRules = TidyUp(policy.GeneralRules);
                                                    
                        policy.SpecificRules = policy.SpecificRules
                                                     .ToDictionary(kvp => kvp.Key, kvp => TidyUp(kvp.Value));
                    }
                });

            services.AddMemoryCache();

            return builder;

            static List<ThrottleRule> TidyUp(IEnumerable<ThrottleRule> rules)
            {
                return rules.GroupBy(kvp => kvp.TimeWindow)
                    .Select(l => l.OrderBy(x => x.Quota))
                    .Select(l => l.First())
                    .OrderByDescending(x => x.TimeWindow)
                    .ToList();
            }
        }

        public static IThrottlerBuilder AddInMemoryCounterStore(this IThrottlerBuilder builder)
        {
            builder.Services.TryAddSingleton<ICounterStore, InMemoryCacheCounterStore>();

            return builder;
        }

        public static IThrottlerBuilder AddDistributedCounterStore(this IThrottlerBuilder builder)
        {
            builder.Services.TryAddSingleton<ICounterStore, DistributedCacheCounterStore>();

            return builder;
        }
    }
}
