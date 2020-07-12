using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ThrottlR
{
    public class ThrottleOptions
    {
        private string _defaultPolicyName = "__DefaultThrottlerPolicy";

        internal IDictionary<string, (ThrottlePolicy policy, Task<ThrottlePolicy> policyTask)> PolicyMap { get; }
            = new Dictionary<string, (ThrottlePolicy, Task<ThrottlePolicy>)>(StringComparer.Ordinal);

        public string DefaultPolicyName
        {
            get => _defaultPolicyName;
            set
            {
                _defaultPolicyName = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Adds a new policy and sets it as the default.
        /// </summary>
        /// <param name="policy">The <see cref="CorsPolicy"/> policy to be added.</param>
        public void AddDefaultPolicy(ThrottlePolicy policy)
        {
            _ = policy ?? throw new ArgumentNullException(nameof(policy));

            AddPolicy(DefaultPolicyName, policy);
        }

        /// <summary>
        /// Adds a new policy and sets it as the default.
        /// </summary>
        /// <param name="configurePolicy">A delegate which can use a policy builder to build a policy.</param>
        public void AddDefaultPolicy(Action<ThrottlePolicyBuilder> configurePolicy)
        {
            _ = configurePolicy ?? throw new ArgumentNullException(nameof(configurePolicy));

            AddPolicy(DefaultPolicyName, configurePolicy);
        }

        /// <summary>
        /// Adds a new policy.
        /// </summary>
        /// <param name="name">The name of the policy.</param>
        /// <param name="policy">The <see cref="ThrottlePolicy"/> policy to be added.</param>
        public void AddPolicy(string name, ThrottlePolicy policy)
        {
            _ = name ?? throw new ArgumentNullException(nameof(name));
            _ = policy ?? throw new ArgumentNullException(nameof(policy));

            PolicyMap[name] = (policy, Task.FromResult(policy));
        }

        /// <summary>
        /// Adds a new policy.
        /// </summary>
        /// <param name="name">The name of the policy.</param>
        /// <param name="configurePolicy">A delegate which can use a policy builder to build a policy.</param>
        public void AddPolicy(string name, Action<ThrottlePolicyBuilder> configurePolicy)
        {
            _ = name ?? throw new ArgumentNullException(nameof(name));
            _ = configurePolicy ?? throw new ArgumentNullException(nameof(configurePolicy));

            var policyBuilder = new ThrottlePolicyBuilder();
            configurePolicy(policyBuilder);
            var policy = policyBuilder.Build();

            PolicyMap[name] = (policy, Task.FromResult(policy));
        }

        /// <summary>
        /// Gets the policy based on the <paramref name="name"/>
        /// </summary>
        /// <param name="name">The name of the policy to lookup.</param>
        /// <returns>The <see cref="ThrottlePolicy"/> if the policy was added.<c>null</c> otherwise.</returns>
        public ThrottlePolicy GetPolicy(string name)
        {
            _ = name ?? throw new ArgumentNullException(nameof(name));

            if (PolicyMap.TryGetValue(name, out var result))
            {
                return result.policy;
            }

            return null;
        }

        /// <summary>
        /// Gets or sets a value indicating whether all requests, including the rejected ones, should be stacked in this order: day, hour, min, sec
        /// </summary>
        public bool StackBlockedRequests { get; set; }
    }
}
