using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ThrottlR
{
    public class AggregateResolver : IResolver
    {
        private readonly List<IResolver> _resolvers;

        public AggregateResolver(IEnumerable<IResolver> resolvers)
        {
            _resolvers = resolvers.ToList();
        }

        public AggregateResolver()
        {
            _resolvers = new List<IResolver>();
        }

        internal void AddResolver(IResolver resolver)
        {
            _resolvers.Add(resolver);
        }

        public async ValueTask<string> ResolveAsync(HttpContext httpContext)
        {
            if (_resolvers.Count == 0)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();

            for (var i = 0; i < _resolvers.Count; i++)
            {
                stringBuilder.Append(await _resolvers[i].ResolveAsync(httpContext));
                stringBuilder.Append("-");
            }

            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            return stringBuilder.ToString();
        }
    }
}
