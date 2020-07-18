using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ThrottlR
{
    public interface IResolver
    {
        Task<string> ResolveAsync(HttpContext httpContext);
    }
}
