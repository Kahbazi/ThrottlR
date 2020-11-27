using System;

namespace ThrottlR.Service
{
    public interface ISystemClock
    {
        DateTime UtcNow { get; }
    }
}
