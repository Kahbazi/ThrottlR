namespace ThrottlR
{
    public class AggregateResolverBuilder
    {
        private readonly AggregateResolver _aggregateResolver;

        public AggregateResolverBuilder()
        {
            _aggregateResolver = new AggregateResolver();
        }

        public AggregateResolverBuilder AppendResolver(IResolver resolver)
        {
            _aggregateResolver.AddResolver(resolver);

            return this;
        }

        public AggregateResolver Build()
        {
            return _aggregateResolver;
        }
    }
}
