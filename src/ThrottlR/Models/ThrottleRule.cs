using System;

namespace ThrottlR
{
    public class ThrottleRule
    {
        /// <summary>
        /// Rate limit period
        /// </summary>
        public TimeSpan Period { get; set; }

        /// <summary>
        /// Maximum number of requests that a client can make in a defined period
        /// </summary>
        public double Limit { get; set; }
    }
}
