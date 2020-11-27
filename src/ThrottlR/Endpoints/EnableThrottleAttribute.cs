using System;

namespace ThrottlR.Endpoints
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class EnableThrottleAttribute : Attribute, IThrottleMetadata
    {
        public EnableThrottleAttribute()
        {

        }

        public EnableThrottleAttribute(string policyName)
        {
            PolicyName = policyName;
        }

        public string PolicyName { get; set; }
    }
}
