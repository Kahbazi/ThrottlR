using System;

namespace ThrottlR
{
    public class ThrottleAttribute : Attribute, IThrottleMetadata
    {
        public string PolicyName { get; set; }
    }
}
