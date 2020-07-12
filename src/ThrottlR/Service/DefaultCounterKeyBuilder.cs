namespace ThrottlR
{
    public class DefaultCounterKeyBuilder : ICounterKeyBuilder
    {
        public string Build(string identity, ThrottleRule rule, string policyName)
        {
            return $"ThrottlR:{policyName}:{rule.TimeWindow}:{identity}";
        }
    }
}
