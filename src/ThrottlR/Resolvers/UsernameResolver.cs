using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ThrottlR
{
    public class UsernameResolver : IResolver
    {
        private static readonly Task<string> _anonymous = Task.FromResult("Anonymous");

        public static UsernameResolver Instance { get; } = new UsernameResolver();

        public Task<string> ResolveAsync(HttpContext httpContext)
        {
            var identity = httpContext.User.Identity;
            if (identity == null || !identity.IsAuthenticated)
            {
                return _anonymous;
            }
            return Task.FromResult(identity.Name);
        }
    }
}
