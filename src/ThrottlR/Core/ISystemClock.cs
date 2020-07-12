using System;

namespace ThrottlR
{
    public interface ISystemClock
    {
        DateTime UtcNow { get; }
    }
}
