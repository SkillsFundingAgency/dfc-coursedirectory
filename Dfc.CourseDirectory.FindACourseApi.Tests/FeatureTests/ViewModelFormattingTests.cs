using Dfc.CourseDirectory.FindACourseApi.Features;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.FindACourseApi.Tests.FeatureTests
{
    public class ViewModelFormattingTests
    {
        [Theory]
        [InlineData(null, null, null)]
        [InlineData("", "", "")]
        [InlineData(" ", " ", " ")]
        [InlineData("", null, null)]
        [InlineData(null, "", null)]
        [InlineData(null, null, "")]
        [InlineData(" ", null, null)]
        [InlineData(null, " ", null)]
        [InlineData(null, null, " ")]
        public void ConcatAddressLines_WithNullOrWhiteSpaceSaonPaonOrStreet_ReturnsEmptyString(string saon, string paon, string street)
        {
            var result = ViewModelFormatting.ConcatAddressLines(saon, paon, street);

            result.Should().Be(string.Empty);
        }

        [Fact]
        public void ConcatAddressLines_WithSaonAndPaonAndStreet_ReturnsSaonPaonAndStreetWithSpaces()
        {
            var result = ViewModelFormatting.ConcatAddressLines("TestSaon", "TestPaon", "TestStreet");

            result.Should().Be("TestSaon TestPaon TestStreet");
        }

        [Fact]
        public void ConcatAddressLines_WithSaonAndNoPaonOrStreet_ReturnsSaon()
        {
            var result = ViewModelFormatting.ConcatAddressLines("TestSaon", null, null);

            result.Should().Be("TestSaon");
        }

        [Fact]
        public void ConcatAddressLines_WithPaonAndNoSaonOrStreet_ReturnsPaon()
        {
            var result = ViewModelFormatting.ConcatAddressLines(null, "TestPaon", null);

            result.Should().Be("TestPaon");
        }

        [Fact]
        public void ConcatAddressLines_WithStreetAndNoSaonOrPaon_ReturnsStreet()
        {
            var result = ViewModelFormatting.ConcatAddressLines(null, null, "TestStreet");

            result.Should().Be("TestStreet");
        }

        [Fact]
        public void ConcatAddressLines_WithSaonAndStreetAndNoPaon_ReturnsSaonAndStreetWithSpace()
        {
            var result = ViewModelFormatting.ConcatAddressLines("TestSaon", null, "TestStreet");

            result.Should().Be("TestSaon TestStreet");
        }

        [Fact]
        public void ConcatAddressLines_WithSaonAndPaonAndNoStreet_ReturnsSaonAndPaonWithSpace()
        {
            var result = ViewModelFormatting.ConcatAddressLines("TestSaon", "TestPaon", null);

            result.Should().Be("TestSaon TestPaon");
        }

        [Fact]
        public void ConcatAddressLines_WithPaonAndStreetAndNoSaon_ReturnsPaonAndStreetWithSpace()
        {
            var result = ViewModelFormatting.ConcatAddressLines(null, "TestPaon", "TestStreet");

            result.Should().Be("TestPaon TestStreet");
        }

        [Fact]
        public void ConcatAddressLines_WithWhiteSpacePaddedSaonAndPaonAndStreet_ReturnsSaonPaonAndStreetWithSpaces()
        {
            var result = ViewModelFormatting.ConcatAddressLines(" TestSaon ", " TestPaon ", " TestStreet ");

            result.Should().Be("TestSaon TestPaon TestStreet");
        }
    }
}
