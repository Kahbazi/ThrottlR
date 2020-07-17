namespace ThrottlR
{
    public class ThrottleMetadata : IThrottleMetadata
    {
        public ThrottleMetadata()
        {
            
        }

        public ThrottleMetadata(string policyName)
        {
            PolicyName = policyName;
        }

        public string PolicyName { get; }
    }
}
