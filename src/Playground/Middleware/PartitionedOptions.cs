using System.Threading.RateLimiting;

namespace Playground
{
    public class PartitionedOptions
    {
        public Dictionary<string, PartitionedRateLimiter<HttpContext>> Policies { get; } = new();

        public void AddPolicy<TPartitionKey>(string policyName, Func<HttpContext, RateLimitPartition<TPartitionKey>> partitioner, IEqualityComparer<TPartitionKey>? equalityComparer = null)
            where TPartitionKey : notnull
        {
            Policies.Add(policyName, PartitionedRateLimiter.Create(partitioner, equalityComparer));
        }
    }

}