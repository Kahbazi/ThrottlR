using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ThrottlR
{
    public interface IResolver
    {
        ValueTask<string> ResolveAsync(HttpContext httpContext);
    }
}
