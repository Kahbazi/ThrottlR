using System;

namespace ThrottlR
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ThrottleAttribute : Attribute, IThrottleMetadata
    {
        public string PolicyName { get; set; }
    }
}
