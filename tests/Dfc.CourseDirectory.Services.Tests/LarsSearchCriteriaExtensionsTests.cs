using System;
using System.Collections.Generic;
using System.Text;
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
            var criteria = new LarsSearchCriteria("some value", null, null, false);

            // act
            var actual = criteria.ToJson();

            // assert
            Assert.Equal(expected, actual);
        }
    }
}
