namespace ThrottlR
{
    public class CounterKeyBuilder : ICounterKeyBuilder
    {
        public string Build(string identity, ThrottleRule rule, string policyName)
        {
            return $"ThrottlR:{policyName}:{rule.Period}:{identity}";
        }
    }
}
