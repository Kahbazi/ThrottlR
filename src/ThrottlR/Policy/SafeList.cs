using System.Collections.Generic;

namespace ThrottlR
{
    public class SafeList : Dictionary<ISafeListResolver, List<string>>
    {
        public void Add(ISafeListResolver resolver, IEnumerable<string> safe)
        {
            if (!TryGetValue(resolver, out var safeScopes))
            {
                safeScopes = new List<string>();
                Add(resolver, safeScopes);
            }

            safeScopes.AddRange(safe);
        }
    }
}
