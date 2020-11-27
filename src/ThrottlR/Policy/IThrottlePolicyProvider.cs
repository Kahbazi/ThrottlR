using System.Threading.Tasks;

namespace ThrottlR.Policy
{
    public interface IThrottlePolicyProvider
    {
        Task<ThrottlePolicy> GetPolicyAsync(string policyName);

    }
}
