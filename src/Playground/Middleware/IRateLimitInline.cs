using System.Threading.RateLimiting;

namespace Playground
{
    public interface IRateLimitInline : IRateLimitMetadata
    {
        PartitionedRateLimiter<HttpContext> RateLimiter { get; }
    }

}