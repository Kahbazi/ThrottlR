using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ThrottlR.Policy
{
    public class SafeListCollectionTests
    {

        [Fact]
        public void SafeListCollection_Add_Multiple_With_Same_Resolver()
        {
            // Arrange
            var safeList = new SafeListCollection();

            // Act
            safeList.AddSafeList(UsernameResolver.Instance, new string[] { "Admin" });
            safeList.AddSafeList(UsernameResolver.Instance, new string[] { "Manager" });

            // Assert
            var kvp = Assert.Single(safeList);
            Assert.Equal(UsernameResolver.Instance, kvp.Key);
            Assert.Equal(2, kvp.Value.Count);
            Assert.Equal(new string[] { "Admin", "Manager" }, kvp.Value);
        }

        [Fact]
        public void SafeListCollection_Add_Handle_Duplicates()
        {
            // Arrange
            var safeList = new SafeListCollection();

            // Act
            safeList.AddSafeList(UsernameResolver.Instance, new string[] { "Admin" });
            safeList.AddSafeList(UsernameResolver.Instance, new string[] { "Admin", "Manager" });
            safeList.AddSafeList(UsernameResolver.Instance, new string[] { "Manager" });

            // Assert
            var kvp = Assert.Single(safeList);
            Assert.Equal(UsernameResolver.Instance, kvp.Key);
            Assert.Equal(2, kvp.Value.Count);
            Assert.Equal(new string[] { "Admin", "Manager" }, kvp.Value);
        }
    }
}
