using System;

namespace ThrottlR.Endpoints
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class DisableThrottleAttribute : Attribute, IDisableThrottle
    {
    }
}
