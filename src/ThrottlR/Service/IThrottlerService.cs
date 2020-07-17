using System.Threading;
using System.Threading.Tasks;

namespace ThrottlR
{
    public interface IThrottlerService
    {
        Task<Counter> ProcessRequestAsync(string counterId, ThrottleRule rule, CancellationToken cancellationToken);
    }
}
