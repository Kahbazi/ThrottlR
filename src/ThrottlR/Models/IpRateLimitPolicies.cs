using System.Collections.Generic;

namespace ThrottlR
{
    public class IpRateLimitPolicies
    {
        public List<IpRateLimitPolicy> IpRules { get; set; } = new List<IpRateLimitPolicy>();
    }
}
