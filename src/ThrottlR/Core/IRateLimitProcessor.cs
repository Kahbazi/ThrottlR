using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottlR
{
    public interface ThrottlerService
    {
        List<ThrottleRule> CombineRules(IReadOnlyList<ThrottleRule> generalRules, IReadOnlyList<ThrottleRule> speceficRules);

        RateLimitHeaders GetRateLimitHeaders(RateLimitCounter counter, ThrottleRule rule);

        Task<RateLimitCounter> ProcessRequestAsync(string counterId, ThrottleRule rule, CancellationToken cancellationToken);
    }
}
