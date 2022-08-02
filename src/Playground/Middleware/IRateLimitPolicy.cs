namespace Playground
{
    public interface IRateLimitPolicy : IRateLimitMetadata
    {
        string PolicyName { get; }
    }

}