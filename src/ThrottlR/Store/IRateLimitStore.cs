using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottlR
{
    public interface IRateLimitStore
    {
        ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken);
        ValueTask<RateLimitCounter?> GetAsync(string key, CancellationToken cancellationToken);
        ValueTask RemoveAsync(string key, CancellationToken cancellationToken);
        ValueTask SetAsync(string key, RateLimitCounter counter, TimeSpan? expirationTime, CancellationToken cancellationToken);
    }
}
