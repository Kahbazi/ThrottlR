using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ThrottlR
{
    public class IpResolver : IResolver
    {
        public static IpResolver Instance { get; } = new IpResolver();

        public Task<string> ResolveAsync(HttpContext httpContext)
        {
            return Task.FromResult(httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty);
        }
    }
}
