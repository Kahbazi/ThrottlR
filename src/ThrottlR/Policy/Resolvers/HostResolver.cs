using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ThrottlR
{
    public class HostResolver : IResolver
    {
        private const string NoHost = "__NoHost__";

        public static HostResolver Instance { get; } = new HostResolver();

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
