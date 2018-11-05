using Xunit;

namespace Dfc.CourseDirectory.Services.Tests
{
    public class LarsSearchCriteriaExtensionsTests
    {
        [Fact]
        public void ToJson_WithSearchOnly_ReturnsJson()
        {
            // arrange
            var expected = "";
            var criteria = new LarsSearchCriteria("some value");

            // act
            var actual = criteria.ToJson();

            // assert
            Assert.Equal(expected, actual);
        }
    }
}