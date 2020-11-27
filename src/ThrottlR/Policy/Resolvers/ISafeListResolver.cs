namespace ThrottlR.Policy.Resolvers
{
    public interface ISafeListResolver : IResolver
    {
        bool Matches(string scope, string safe);
    }
}
