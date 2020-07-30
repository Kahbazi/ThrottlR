using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ThrottlR
{
    public class TypeResolver<TResolver> : IResolver where TResolver : IResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public TypeResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ValueTask<string> ResolveAsync(HttpContext httpContext)
        {
            return _serviceProvider.GetService<TResolver>().ResolveAsync(httpContext);
        }
    }
}
