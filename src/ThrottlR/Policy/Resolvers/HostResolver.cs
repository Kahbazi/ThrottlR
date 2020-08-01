using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ThrottlR
{
    public class HostResolver : ISafeListResolver
    {
        private const string NoHost = "__NoHost__";

        private HostResolver()
        {

        }

        public static HostResolver Instance { get; } = new HostResolver();

        public bool Matches(string scope, string safe)
        {
            return scope.Equals(safe, StringComparison.InvariantCultureIgnoreCase);
        }

        public ValueTask<string> ResolveAsync(HttpContext httpContext)
        {
            if (httpContext.Request.Host.HasValue)
            {
                return new ValueTask<string>(httpContext.Request.Host.Value);
            }
            else
            {
                return new ValueTask<string>(NoHost);
            }
        }
    }
}
