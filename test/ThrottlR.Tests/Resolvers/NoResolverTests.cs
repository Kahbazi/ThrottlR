using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace ThrottlR.Resolvers
{
    public class NoResolverTests
    {
        [Fact]
        public async Task NoResolver_Always_Returns_Star()
        {
            var resolver = new NoResolver();

            var httpContext = new DefaultHttpContext();
            var identity = await resolver.ResolveAsync(httpContext);

            Assert.Equal("*", identity);
        }
    }
}
