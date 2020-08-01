using Xunit;

namespace ThrottlR
{
    public class IpResolverTests
    {
        [Theory]
        [InlineData("10.20.10.47", "10.20.10.0/24")]
        [InlineData("10.20.10.47", "10.20.0.0/16")]
        [InlineData("10.20.10.47", "10.0.0.0/8")]
        [InlineData("10.20.10.47", "10.20.10.47")]
        public void IpResolver_Matches_True(string scope, string safe)
        {
            // Arrange
            var resolver = IpResolver.Instance;

            // Act
            var result = resolver.Matches(scope, safe);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("10.20.10.47", "10.20.80.0/24")]
        [InlineData("10.20.10.47", "10.80.0.0/16")]
        [InlineData("10.20.10.47", "80.0.0.0/8")]
        [InlineData("10.20.10.47", "10.20.10.80")]
        public void IpResolver_Matches_False(string scope, string safe)
        {
            // Arrange
            var resolver = IpResolver.Instance;

            // Act
            var result = resolver.Matches(scope, safe);

            // Assert
            Assert.False(result);

        }
    }
}
