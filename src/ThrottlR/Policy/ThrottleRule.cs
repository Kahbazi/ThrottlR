using System;

namespace ThrottlR.Policy
{
    /// <summary>
    /// Limit the number of acceptable requests in a given time window.
    /// </summary>
    public struct ThrottleRule
    {
        /// <summary>
        ///
        /// </summary>
        public TimeSpan TimeWindow { get; set; }

        /// <summary>
        ///
        /// </summary>
        public double Quota { get; set; }

        public override string ToString()
        {
            return $"{Quota};w={TimeWindow.TotalSeconds}";
        }
    }
}
