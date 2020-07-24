namespace ThrottlR
{
    public class DefaultCounterKeyBuilder : ICounterKeyBuilder
    {
        public string Build(string identity, ThrottleRule rule, string policyName, string endpointName)
        {
            return $"ThrottlR:{endpointName}:{policyName}:{rule.TimeWindow}:{identity}";
        }
    }
}
