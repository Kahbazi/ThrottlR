using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThrottlR.Models;
using ThrottlR.Policy;

namespace ThrottlR.Service
{
    public interface IThrottlerService
    {
        IEnumerable<ThrottleRule> GetRules(IReadOnlyList<ThrottleRule> generalRules, IReadOnlyList<ThrottleRule> specificRules);

        Task<Counter> ProcessRequestAsync(ThrottlerItem throttlerItem, ThrottleRule rule, CancellationToken cancellationToken);
    }
}
