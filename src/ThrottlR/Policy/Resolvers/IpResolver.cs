using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ThrottlR
{
    public class IpResolver : ISafeListResolver
    {
        private IpResolver()
        {

        }

        public static IpResolver Instance { get; } = new IpResolver();

        public bool Matches(string scope, string safe)
        {
            var parts = safe.Split('/');
            if (parts.Length == 2)
            {
                var safeMask = SubnetMask.CreateByNetBitLength(int.Parse(parts[1]));
                var safeIp = IPAddress.Parse(parts[0]);

                var scopeIp = IPAddress.Parse(scope);

                return scopeIp.IsInSameSubnet(safeIp, safeMask);
            }

            return scope.Equals(safe, StringComparison.InvariantCultureIgnoreCase);
        }

        public ValueTask<string> ResolveAsync(HttpContext httpContext)
        {
            return new ValueTask<string>(httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty);
        }
    }
}
