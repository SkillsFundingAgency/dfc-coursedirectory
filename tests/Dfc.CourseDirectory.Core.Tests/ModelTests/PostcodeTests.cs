using System;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.ModelTests
{
    public class PostcodeTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("x")]
        public void TryParse_InvalidPostcode_ReturnsFalse(string input)
        {
            // Arrange
            Postcode postcode;

            // Act
            var result = Postcode.TryParse(input, out postcode);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("x")]
        public void Constructor_InvalidPostcode_ThrowsArgumentException(string input)
        {
            // Arrange

            // Act
            var ex = Record.Exception(() => new Postcode(input));

            // Assert
            ex.Should().NotBeNull();
            ex.Should().BeOfType<ArgumentException>().Subject.Message.Should().Be("Input is not a valid UK postcode. (Parameter 'value')");
        }

        // Examples from https://en.wikipedia.org/wiki/Postcodes_in_the_United_Kingdom
        [Theory]
        [InlineData("EC1A 1BB", "EC1A 1BB")]
        [InlineData("EC1A1BB", "EC1A 1BB")]
        [InlineData("ec1a 1bb", "EC1A 1BB")]
        [InlineData("ec1a1bb", "EC1A 1BB")]
        [InlineData("W1A 0AX", "W1A 0AX")]
        [InlineData("W1A0AX", "W1A 0AX")]
        [InlineData("w1a 0ax", "W1A 0AX")]
        [InlineData("w1a0ax", "W1A 0AX")]
        [InlineData("M1 1AE", "M1 1AE")]
        [InlineData("M11AE", "M1 1AE")]
        [InlineData("m1 1ae", "M1 1AE")]
        [InlineData("m11ae", "M1 1AE")]
        public void TryParse_ValidPostcode_ReturnsTrueAndOutputsNonNullNormalizedPostcode(
            string input,
            string expectedNormalizedPostcode)
        {
            // Arrange
            Postcode postcode;

            // Act
            var result = Postcode.TryParse(input, out postcode);

            // Assert
            result.Should().BeTrue();
            postcode.ToString().Should().Equals(expectedNormalizedPostcode);
        }

        [Theory]
        [InlineData("EC1A 1BB", "EC1A 1BB", true)]
        [InlineData("EC1A 1BB", "ec1a 1bb", true)]
        [InlineData("EC1A1BB", "ec1a 1bb", true)]
        [InlineData("EC1A1BB", "M1 1AE", false)]
        public void EqualityOperator_ReturnsExpectedResult(string left, string right, bool expectedResult)
        {
            // Arrange
            var leftPostcode = new Postcode(left);
            var rightPostcode = new Postcode(right);

            // Act
            var result = leftPostcode == rightPostcode;

            // Assert
            result.Should().Equals(expectedResult);
        }

        [Theory]
        [InlineData("EC1A 1BB", "EC1A 1BB", false)]
        [InlineData("EC1A 1BB", "ec1a 1bb", false)]
        [InlineData("EC1A1BB", "ec1a 1bb", false)]
        [InlineData("EC1A1BB", "M1 1AE", true)]
        public void InequalityOperator_ReturnsExpectedResult(string left, string right, bool expectedResult)
        {
            // Arrange
            var leftPostcode = new Postcode(left);
            var rightPostcode = new Postcode(right);

            // Act
            var result = leftPostcode != rightPostcode;

            // Assert
            result.Should().Equals(expectedResult);
        }

        [Theory]
        [MemberData(nameof(Equals_ReturnsExpectedResultData))]
        public void Equals_ReturnsExpectedResult(Postcode left, object right, bool expectedResult)
        {
            // Arrange

            // Act
            var result = left.Equals(right);

            // Assert
            result.Should().Equals(expectedResult);
        }

        public static TheoryData<Postcode, object, bool> Equals_ReturnsExpectedResultData => new TheoryData<Postcode, object, bool>()
        {
            { new Postcode("EC1A 1BB"), "EC1A 1BB", true },
            { new Postcode("EC1A 1BB"), new Postcode("ec1a 1bb"), true },
            { new Postcode("EC1A1BB"), new Postcode("ec1a 1bb"), true },
            { new Postcode("EC1A1BB"), new Postcode("M1 1AE"), false },
            { new Postcode("EC1A1BB"), string.Empty, false },
            { new Postcode("EC1A1BB"), null, false },
        };
    }
}
