namespace ThrottlR
{
    public static class SafeListBuilderExtensions
    {
        public static ThrottlePolicyBuilder IP(this SafeListBuilder builder, params string[] safe)
        {
            return builder.ForResolver(IpResolver.Instance, safe);
        }

        public static ThrottlePolicyBuilder User(this SafeListBuilder builder, params string[] safe)
        {
            return builder.ForResolver(UsernameResolver.Instance, safe);
        }

        public static ThrottlePolicyBuilder Host(this SafeListBuilder builder, params string[] safe)
        {
            return builder.ForResolver(HostResolver.Instance, safe);
        }
    }
}
