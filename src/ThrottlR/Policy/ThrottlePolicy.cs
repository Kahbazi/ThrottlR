using System.Collections.Generic;

namespace ThrottlR
{
    public class ThrottlePolicy
    {
        public ThrottlePolicy()
        {
            GeneralRules = new List<ThrottleRule>();
            SafeList = new SafeList();
            SpecificRules = new Dictionary<string, List<ThrottleRule>>();
            Resolver = NoResolver.Instance;
        }

        public List<ThrottleRule> GeneralRules { get; set; }

        public SafeList SafeList { get; set; }

        public Dictionary<string, List<ThrottleRule>> SpecificRules { get; set; }

        public IResolver Resolver { get; set; }

        public bool ApplyPerEndpoint { get; set; }
    }
}
