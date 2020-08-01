using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace ThrottlR
{
    public class AccessTokenResolver : IResolver
    {
        private AccessTokenResolver()
        {
            
        }

        public static AccessTokenResolver Instance { get; } = new AccessTokenResolver();

        public ValueTask<string> ResolveAsync(HttpContext httpContext)
        {
            return new ValueTask<string>(httpContext.GetTokenAsync("access_token"));
        }
    }
}
