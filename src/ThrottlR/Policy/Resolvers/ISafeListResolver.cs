namespace ThrottlR
{
    public interface ISafeListResolver : IResolver
    {
        bool Matches(string scope, string safe);
    }
}
