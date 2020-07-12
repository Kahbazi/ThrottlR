using System;

namespace ThrottlR
{
    public class ThrottleMetadataAttribute : Attribute, IThrottleMetadata
    {
        public string PolicyName { get; set; }
    }
}
