using ThrottlR.Policy.Resolvers;

namespace ThrottlR.Policy
{
    public class SafeListBuilder
    {
        private readonly ThrottlePolicyBuilder _throttlePolicyBuilder;
        private readonly SafeListCollection _safeList;

        public SafeListBuilder(ThrottlePolicyBuilder throttlePolicyBuilder, SafeListCollection safeList)
        {
            _throttlePolicyBuilder = throttlePolicyBuilder;
            _safeList = safeList;
        }

        public ThrottlePolicyBuilder ForResolver(ISafeListResolver resolver, params string[] safe)
        {
            _safeList.AddSafeList(resolver, safe);

            return _throttlePolicyBuilder;
        }
    }
}
