using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Xunit;
using System.Threading;

namespace ThrottlR.Tests
{
    public class RateLimitProcessorTests
    {
        private static (ThrottlerService processor, TimeMachine timeMachine) CreateProcessor()
        {
            var store = new TestRateLimitStore();
            var clock = new TimeMachine();
            return (new ThrottlerService(store, clock), clock);
        }
    }
}
