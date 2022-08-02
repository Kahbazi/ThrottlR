namespace Playground
{
    public class RateLimitPolicy : IRateLimitPolicy
    {
        public RateLimitPolicy(string policyName)
        {
            PolicyName = policyName;
        }

        public string PolicyName { get; }
    }

}