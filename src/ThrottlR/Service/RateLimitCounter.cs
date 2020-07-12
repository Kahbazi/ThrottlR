using System;

namespace ThrottlR
{
    /// <summary>
    /// Stores the initial access time and the numbers of calls made from that point
    /// </summary>
    public readonly struct RateLimitCounter
    {
        public RateLimitCounter(DateTime timestamp, double count)
        {
            Timestamp = timestamp;
            Count = count;
        }

        public DateTime Timestamp { get; }

        public double Count { get; }
    }
}