using ThrottlR;

namespace Microsoft.AspNetCore.Builder
{
    public static class ThrottlerApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseThrottler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ThrottlerMiddleware>();
        }
    }
}
