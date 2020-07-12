using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottlR
{
    public interface IThrottlerService
    {
        Task<RateLimitCounter> ProcessRequestAsync(string counterId, ThrottleRule rule, CancellationToken cancellationToken);
    }
}
