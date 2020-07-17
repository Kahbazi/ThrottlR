using System;

namespace ThrottlR
{
    /// <summary>
    /// Stores the initial access time and the numbers of calls made from that point
    /// </summary>
    public readonly struct Counter
    {
        public Counter(DateTime timestamp, int count)
        {
            Timestamp = timestamp;
            Count = count;
        }

        public DateTime Timestamp { get; }

        public int Count { get; }
    }
}
