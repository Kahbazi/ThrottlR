using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ThrottlR
{
    public class NoResolver : IResolver
    {
        private static readonly Task<string> _identity = Task.FromResult("*");

        public static NoResolver Instance { get; } = new NoResolver();

        public Task<string> ResolveAsync(HttpContext httpContext)
        {
            return _identity;
        }
    }
}
