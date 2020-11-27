using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ThrottlR.Policy.Resolvers
{
    public interface IResolver
    {
        ValueTask<string> ResolveAsync(HttpContext httpContext);
    }
}
