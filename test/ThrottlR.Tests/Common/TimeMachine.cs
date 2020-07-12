using System;

namespace ThrottlR.Tests
{
    public class TimeMachine : ISystemClock
    {
        public TimeMachine()
        {
            UtcNow = DateTime.UtcNow;
        }

        public DateTime UtcNow { get; set; }

        public void Travel(DateTime dateTime)
        {
            UtcNow = dateTime;
        }

        public void Travel(TimeSpan timeSpan)
        {
            UtcNow += timeSpan;
        }
    }
}
