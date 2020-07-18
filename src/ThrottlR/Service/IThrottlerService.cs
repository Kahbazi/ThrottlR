using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottlR
{
    public interface IThrottlerService
    {
        IEnumerable<ThrottleRule> GetRules(ThrottlePolicy policy, string identity);

        Task<Counter> ProcessRequestAsync(string counterId, ThrottleRule rule, CancellationToken cancellationToken);
    }
}
