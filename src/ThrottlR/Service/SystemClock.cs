using System;

namespace ThrottlR
{
    /// <summary>
    /// Provides access to the normal system clock.
    /// </summary>
    internal class SystemClock : ISystemClock
    {
        /// <summary>
        /// Retrieves the current UTC system time.
        /// </summary>
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
