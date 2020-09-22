using Dfc.CourseDirectory.Core.Search.AzureSearch;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.Search.AzureSearch
{
    public class SearchTextExtensionsTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void RemoveNonAlphanumericChars_WithNullOrEmptySearchText_ReturnsEmptyString(string searchText)
        {
            var result = searchText.RemoveNonAlphanumericChars();

            result.Should().Be(string.Empty);
        }

        [Fact]
        public void RemoveNonAlphanumericChars_RemovesNonAlphanumericOrWhiteSpaceChars()
        {
            var searchText = @"TestSearch!""£$%^&*()_+-=`{}[]:@~;'#<>?,./|\' Text";

            var result = searchText.RemoveNonAlphanumericChars();

            result.Should().Be("TestSearch Text");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public void ApplyWildcardsToAllSegments_WithNullOrEmptyOrWhiteSpaceSearchText_ReturnsWildcard(string searchText)
        {
            var result = searchText.ApplyWildcardsToAllSegments();

            result.Should().Be("*");
        }

        [Fact]
        public void ApplyWildcardsToAllSegments_WithSingleSegment_ReturnsSearchTextWithWildcard()
        {
            var searchText = "TestSearchText";

            var result = searchText.ApplyWildcardsToAllSegments();

            result.Should().Be("TestSearchText*");
        }

        [Fact]
        public void ApplyWildcardsToAllSegments_WithMultipleSegments_ReturnsSearchTextWithWildcardsForEachSegments()
        {
            var searchText = "Test Search Text";

            var result = searchText.ApplyWildcardsToAllSegments();

            result.Should().Be("Test* Search* Text*");
        }

        [Fact]
        public void ApplyWildcardsToAllSegments_WithSegmentDelimiter_ReturnsSearchTextWithWildcardsForEachSegments()
        {
            var searchText = "Test,Search,Text";

            var result = searchText.ApplyWildcardsToAllSegments(",");

            result.Should().Be("Test*,Search*,Text*");
        }
    }
}