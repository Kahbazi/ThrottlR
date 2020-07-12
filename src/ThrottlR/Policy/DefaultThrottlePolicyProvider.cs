using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace ThrottlR
{
    public class DefaultThrottlePolicyProvider : IThrottlePolicyProvider
    {
        private static readonly Task<ThrottlePolicy> _nullResult = Task.FromResult<ThrottlePolicy>(null);
        private readonly ThrottleOptions _options;

        /// <summary>
        /// Creates a new instance of <see cref="DefaultCorsPolicyProvider"/>.
        /// </summary>
        /// <param name="options">The options configured for the application.</param>
        public DefaultThrottlePolicyProvider(IOptions<ThrottleOptions> options)
        {
            _options = options.Value;
        }

        /// <inheritdoc />
        public Task<ThrottlePolicy> GetPolicyAsync(string policyName)
        {
            policyName ??= _options.DefaultPolicyName;
            if (_options.PolicyMap.TryGetValue(policyName, out var result))
            {
                return result.policyTask;
            }

            return _nullResult;
        }
    }
}
