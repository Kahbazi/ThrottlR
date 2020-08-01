namespace ThrottlR
{
    public class SafeListBuilder
    {
        private readonly ThrottlePolicyBuilder _throttlePolicyBuilder;
        private readonly SafeList _safeList;

        public SafeListBuilder(ThrottlePolicyBuilder throttlePolicyBuilder, SafeList safeList)
        {
            _throttlePolicyBuilder = throttlePolicyBuilder;
            _safeList = safeList;
        }

        public ThrottlePolicyBuilder ForResolver(ISafeListResolver resolver, params string[] safe)
        {
            _safeList.Add(resolver, safe);

            return _throttlePolicyBuilder;
        }
    }
}
