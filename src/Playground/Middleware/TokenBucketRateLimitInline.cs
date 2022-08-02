using System.Threading.RateLimiting;

namespace Playground
{
    public class TokenBucketRateLimitInline : IRateLimitInline
    {
        public TokenBucketRateLimitInline(int tokenlimit, TimeSpan replenishmentPeriod, int tokensPerPeriod)
            : this(_ => string.Empty, tokenlimit, replenishmentPeriod, tokensPerPeriod)
        {

        }

        public TokenBucketRateLimitInline(Func<HttpContext, string> partitioner, int tokenlimit, TimeSpan replenishmentPeriod, int tokensPerPeriod)
        {
            RateLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var partition = partitioner(context);
                return RateLimitPartition.CreateTokenBucketLimiter<string>(partition, key =>
                {
                    return new TokenBucketRateLimiterOptions(
                        tokenLimit: tokenlimit,
                        queueProcessingOrder: QueueProcessingOrder.OldestFirst,
                        queueLimit: 0,
                        replenishmentPeriod: replenishmentPeriod,
                        tokensPerPeriod: tokensPerPeriod
                    );
                });
            });
        }

        public PartitionedRateLimiter<HttpContext> RateLimiter { get; }
    }

}