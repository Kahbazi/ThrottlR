using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottlR
{
    public interface ICounterStore
    {
        ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken);
        ValueTask<Counter?> GetAsync(string key, CancellationToken cancellationToken);
        ValueTask RemoveAsync(string key, CancellationToken cancellationToken);
        ValueTask SetAsync(string key, Counter counter, TimeSpan? expirationTime, CancellationToken cancellationToken);
    }
}
