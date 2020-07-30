using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ThrottlR
{
    public class UsernameResolver : IResolver
    {
        private const string Anonymous = "__Anonymous__";

        public static UsernameResolver Instance { get; } = new UsernameResolver();

        public ValueTask<string> ResolveAsync(HttpContext httpContext)
        {
            var identity = httpContext.User?.Identity;
            if (identity == null || !identity.IsAuthenticated)
            {
                return new ValueTask<string>(Anonymous);
            }
            return new ValueTask<string>(identity.Name);
        }
    }
}
