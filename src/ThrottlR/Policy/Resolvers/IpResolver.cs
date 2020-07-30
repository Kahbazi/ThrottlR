using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ThrottlR
{
    public class IpResolver : IResolver
    {
        public static IpResolver Instance { get; } = new IpResolver();

        public ValueTask<string> ResolveAsync(HttpContext httpContext)
        {
            return new ValueTask<string>(httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty);
        }
    }
}
