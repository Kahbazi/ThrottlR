using System;
using System.Threading;
using System.Threading.Tasks;
using ThrottlR.Models;

namespace ThrottlR.Service.Store
{
    public interface ICounterStore
    {
        ValueTask<Counter?> GetAsync(ThrottlerItem throttlerItem, CancellationToken cancellationToken);

        ValueTask RemoveAsync(ThrottlerItem throttlerItem, CancellationToken cancellationToken);

        ValueTask SetAsync(ThrottlerItem throttlerItem, Counter counter, TimeSpan? expirationTime, CancellationToken cancellationToken);
    }
}
