using System.Collections.Generic;

namespace ThrottlR
{
    public class SafeListCollection : Dictionary<ISafeListResolver, List<string>>
    {
        public void AddSafeList(ISafeListResolver resolver, params string[] safeList)
        {
            if (!TryGetValue(resolver, out var safeScopes))
            {
                safeScopes = new List<string>();
                Add(resolver, safeScopes);
            }

            for (var i = 0; i < safeList.Length; i++)
            {
                var item = safeList[i];
                if (!safeScopes.Contains(item))
                {
                    safeScopes.Add(item);
                }
            }
        }
    }
}
