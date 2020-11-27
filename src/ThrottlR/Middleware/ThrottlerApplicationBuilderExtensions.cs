using Microsoft.AspNetCore.Builder;

namespace ThrottlR.Middleware
{
    public static class ThrottlerApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseThrottler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ThrottlerMiddleware>();
        }
    }
}
