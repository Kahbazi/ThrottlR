namespace ThrottlR
{
    public interface ICounterKeyBuilder
    {
        string Build(string identity, ThrottleRule rule, string policyName, string endpointName);
    }
}
