using Xunit;
using Xunit.Extensions;

namespace AttributeAuthorization.Tests
{
    public class AuthorizedForTests
    {
        [Theory]
        [InlineData("permission", "permission")]
        [InlineData("parent:permission", "parent,parent:permission")]
        [InlineData(":permission", ":permission")]
        public void Expand(string permission, string expectedPermissions)
        {
            var expected = expectedPermissions.Split(new[] { ',' });
            var af = new RequiresAuth(permission);

            var actual = af.Expand();

            Assert.Equal(expected, actual);
        }
    }
}
