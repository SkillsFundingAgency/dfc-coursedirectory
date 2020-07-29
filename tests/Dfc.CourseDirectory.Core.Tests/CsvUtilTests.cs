using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests
{
    public class CsvUtilTests
    {
        [Theory]
        [MemberData(nameof(GetCountLinesData))]
        public void CountLines(string lines, int expectedResult)
        {
            // Arrange
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(lines));
            stream.Seek(0L, SeekOrigin.Begin);

            // Act
            var lineCount = CsvUtil.CountLines(stream);

            // Assert
            Assert.Equal(expectedResult, lineCount);
        }

        public static IEnumerable<object[]> GetCountLinesData()
        {
            yield return new object[]
            {
                "",
                0
            };

            yield return new object[]
            {
                "one line",
                1
            };

            yield return new object[]
            {
                "first line\nsecond line\nthird line",
                3
            };

            yield return new object[]
            {
                "first line\nsecond line\nthird line\n",
                3
            };

            yield return new object[]
            {
                "first line\r\nsecond line\r\nthird line",
                3
            };
        }
    }
}