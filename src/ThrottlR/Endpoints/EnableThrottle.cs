namespace ThrottlR
{
    public class EnableThrottle : IThrottleMetadata
    {
        public EnableThrottle()
        {
            
        }

        public EnableThrottle(string policyName)
        {
            PolicyName = policyName;
        }

        public string PolicyName { get; }
    }
}
