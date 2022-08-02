using System.Threading.RateLimiting;

namespace Playground
{
    public class RateLimitInline : IRateLimitInline
    {
        public RateLimitInline(PartitionedRateLimiter<HttpContext> rateLimiter)
        {
            RateLimiter = rateLimiter;
        }

        public PartitionedRateLimiter<HttpContext> RateLimiter { get; }
    }

}