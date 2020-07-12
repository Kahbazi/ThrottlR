using System.Collections.Generic;

namespace ThrottlR
{
    public class RateLimitPolicy
    {
        public List<ThrottleRule> Rules { get; set; } = new List<ThrottleRule>();
    }
}
