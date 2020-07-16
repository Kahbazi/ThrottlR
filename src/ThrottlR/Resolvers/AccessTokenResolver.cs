using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace ThrottlR
{
    public class AccessTokenResolver : IResolver
    {
        public static AccessTokenResolver Instance { get; } = new AccessTokenResolver();

        public Task<string> ResolveAsync(HttpContext httpContext)
        {
            return httpContext.GetTokenAsync("access_token");
        }
    }
}
