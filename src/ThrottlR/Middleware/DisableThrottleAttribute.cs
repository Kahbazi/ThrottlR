using System;

namespace ThrottlR
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class DisableThrottleAttribute : Attribute, IDisableThrottle
    {
    }
}
