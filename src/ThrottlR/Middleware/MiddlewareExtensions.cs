using ThrottlR;

namespace Microsoft.AspNetCore.Builder
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseThrottler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ThrottlerMiddleware>();
        }
    }
}
