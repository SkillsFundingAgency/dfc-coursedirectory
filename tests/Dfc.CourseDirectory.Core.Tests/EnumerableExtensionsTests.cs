using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests
{
    public class EnumerableExtensionsTests
    {
        [Theory]
        [MemberData(nameof(ToCommaSeparatedStringData))]
        public void ToCommaSeparatedString(IEnumerable<string> values, string expectedResult)
        {
            // Arrange

            // Act
            var result = values.ToCommaSeparatedString();

            // Assert
            result.Should().Be(expectedResult);
        }

        public static TheoryData<IEnumerable<string>, string> ToCommaSeparatedStringData { get; } = new TheoryData<IEnumerable<string>, string>()
        {
            { new string[0], string.Empty },
            { new[] { "first" }, "first" },
            { new[] { "first", "second" }, "first and second" },
            { new[] { "first", "second", "third" }, "first, second and third" },
            { new[] { "first", "second", "third", "fourth" }, "first, second, third and fourth" },
        };
    }
}
