using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace ThrottlR.Resolvers
{
    public class UsernameResolverTests
    {
        [Fact]
        public async Task UsernameResolver_Anonymous_When_User_Is_Null()
        {
            var resolver = new UsernameResolver();

            var httpContext = new DefaultHttpContext();
            var identity = await resolver.ResolveAsync(httpContext);

            Assert.Equal("__Anonymous__", identity);
        }

        [Fact]
        public async Task UsernameResolver_Anonymous_When_Identity_Is_Null()
        {
            var resolver = new UsernameResolver();

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal();
            var identity = await resolver.ResolveAsync(httpContext);

            Assert.Equal("__Anonymous__", identity);
        }

        [Fact]
        public async Task UsernameResolver_Anonymous_When_IsAuthenticated_Is_False()
        {
            var resolver = new UsernameResolver();

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new[] { new ClaimsIdentity() });
            var identity = await resolver.ResolveAsync(httpContext);

            Assert.Equal("__Anonymous__", identity);
        }

        [Fact]
        public async Task UsernameResolver_Username()
        {
            var resolver = new UsernameResolver();

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { new Claim("user", "admin") }, "test", "user", "role") });
            var identity = await resolver.ResolveAsync(httpContext);

            Assert.Equal("admin", identity);
        }
    }
}
