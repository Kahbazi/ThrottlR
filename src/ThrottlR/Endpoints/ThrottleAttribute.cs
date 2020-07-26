using System;
using System.Collections.Generic;

namespace ThrottlR
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ThrottleAttribute : Attribute, IThrottleRulesMetadata
    {
        private static readonly TimeSpan _oneSecond = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan _oneMinute = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan _oneHour = TimeSpan.FromHours(1);
        private static readonly TimeSpan _oneDay = TimeSpan.FromDays(1);

        private readonly object _lockObject = new object();

        private List<ThrottleRule> _generalRules;

        IReadOnlyList<ThrottleRule> IThrottleRulesMetadata.GeneralRules
        {
            get
            {
                if (_generalRules == null)
                {
                    lock (_lockObject)
                    {
                        var generalRules = new List<ThrottleRule>();

                        if (PerDay > 0)
                        {
                            generalRules.Add(new ThrottleRule { Quota = PerDay, TimeWindow = _oneDay });
                        }

                        if (PerHour > 0)
                        {
                            generalRules.Add(new ThrottleRule { Quota = PerHour, TimeWindow = _oneHour });
                        }

                        if (PerMinute > 0)
                        {
                            generalRules.Add(new ThrottleRule { Quota = PerMinute, TimeWindow = _oneMinute });
                        }

                        if (PerSecond > 0)
                        {
                            generalRules.Add(new ThrottleRule { Quota = PerSecond, TimeWindow = _oneSecond });
                        }

                        _generalRules = generalRules;
                    }
                }

                return _generalRules;
            }
        }

        public long PerSecond { get; set; }

        public long PerMinute { get; set; }

        public long PerHour { get; set; }

        public long PerDay { get; set; }

        public string PolicyName { get; set; }
    }
}
