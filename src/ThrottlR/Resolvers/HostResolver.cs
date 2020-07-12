using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ThrottlR
{
    public class HostResolver : IResolver
    {
        private static readonly Task<string> _noHost = Task.FromResult("NoHost");

        public static HostResolver Instance { get; } = new HostResolver();

        public Task<string> ResolveAsync(HttpContext httpContext)
        {
            if (httpContext.Request.Host.HasValue)
            {
                return Task.FromResult(httpContext.Request.Host.Value);
            }
            else
            {
                return _noHost;
            }
        }
    }
}
