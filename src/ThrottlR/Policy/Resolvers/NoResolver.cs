using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ThrottlR
{
    public class NoResolver : IResolver
    {
        private const string Identity = "*";

        public static NoResolver Instance { get; } = new NoResolver();

        public ValueTask<string> ResolveAsync(HttpContext httpContext)
        {
            return new ValueTask<string>(Identity);
        }
    }
}
