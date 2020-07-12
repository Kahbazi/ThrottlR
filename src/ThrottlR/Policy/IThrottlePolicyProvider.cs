using System.Threading.Tasks;

namespace ThrottlR
{
    public interface IThrottlePolicyProvider
    {
        Task<ThrottlePolicy> GetPolicyAsync(string policyName);

    }
}
