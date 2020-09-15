using System.Linq;
using Azure.Search.Documents.Models;
using Dfc.CourseDirectory.Core.Search;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.Search
{
    public class ProviderSearchQueryTests
    {
        [Fact]
        public void GenerateSearchQuery_GeneratesExpectedSearchQuery()
        {
            var query = new ProviderSearchQuery
            {
                SearchText = "TestSearchText",
                Towns = new[] { "TestTown1", "TestTown2", "TestTown3" },
                Size = 123
            };

            var result = query.GenerateSearchQuery();

            result.SearchText.Should().Be("TestSearchText*");
            result.Options.SearchMode.Should().Be(SearchMode.All);
            result.Options.Filter.Should().Be("search.in(Town, 'TestTown1|TestTown2|TestTown3', '|')");
            result.Options.Size.Should().Be(123);
        }

        [Theory]
        [InlineData(null, "*")]
        [InlineData("", "*")]
        [InlineData(" ", "*")]
        [InlineData("TestSearchText", "TestSearchText*")]
        [InlineData(" TestSearchText", "TestSearchText*")]
        [InlineData("TestSearchText ", "TestSearchText*")]
        public void GenerateSearchQuery_WithSearchText_ReturnsQueryWithExpectedSearchText(string searchText, string expectedResult)
        {
            var query = new ProviderSearchQuery
            {
                SearchText = searchText
            };

            var result = query.GenerateSearchQuery();

            result.SearchText.Should().Be(expectedResult);
        }

        [Fact]
        public void GenerateSearchQuery_ReturnsQueryWithSearchModeAll()
        {
            var query = new ProviderSearchQuery
            {
                SearchText = "TestSearchText"
            };

            var result = query.GenerateSearchQuery();

            result.Options.SearchMode.Should().Be(SearchMode.All);
        }

        [Fact]
        public void GenerateSearchQuery_WithNullTowns_ReturnsQueryWithEmptyFilter()
        {
            var query = new ProviderSearchQuery
            {
                SearchText = "TestSearchText"
            };

            var result = query.GenerateSearchQuery();

            result.Options.Filter.Should().Be(string.Empty);
        }

        [Fact]
        public void GenerateSearchQuery_WithEmptyTowns_ReturnsQueryWithEmptyFilter()
        {
            var query = new ProviderSearchQuery
            {
                SearchText = "TestSearchText",
                Towns = Enumerable.Empty<string>()
            };

            var result = query.GenerateSearchQuery();

            result.Options.Filter.Should().Be(string.Empty);
        }

        [Fact]
        public void GenerateSearchQuery_WithSingleTown_ReturnsQueryWithExpectedSearchInFilter()
        {
            var query = new ProviderSearchQuery
            {
                SearchText = "TestSearchText",
                Towns = new[] { "TestTown1" }
            };

            var result = query.GenerateSearchQuery();

            result.Options.Filter.Should().Be("search.in(Town, 'TestTown1', '|')");
        }

        [Fact]
        public void GenerateSearchQuery_WithMultipleTowns_ReturnsQueryWithExpectedSearchInFilter()
        {
            var query = new ProviderSearchQuery
            {
                SearchText = "TestSearchText",
                Towns = new[] { "TestTown1", "TestTown2", "TestTown3" }
            };

            var result = query.GenerateSearchQuery();

            result.Options.Filter.Should().Be("search.in(Town, 'TestTown1|TestTown2|TestTown3', '|')");
        }

        [Fact]
        public void GenerateSearchQuery_WithDuplicateTowns_ReturnsQueryWithExpectedSearchInFilter()
        {
            var query = new ProviderSearchQuery
            {
                SearchText = "TestSearchText",
                Towns = new[] { "TestTown1", "TestTown2", "TestTown1" }
            };

            var result = query.GenerateSearchQuery();

            result.Options.Filter.Should().Be("search.in(Town, 'TestTown1|TestTown2', '|')");
        }

        [Fact]
        public void GenerateSearchQuery_WithSize_ReturnsQueryWithSize()
        {
            var query = new ProviderSearchQuery
            {
                SearchText = "TestSearchText",
                Size = 123
            };

            var result = query.GenerateSearchQuery();

            result.Options.Size.Should().Be(123);
        }

        [Fact]
        public void GenerateSearchQuery_WithNullSize_ReturnsQueryWithSize20()
        {
            var query = new ProviderSearchQuery
            {
                SearchText = "TestSearchText"
            };

            var result = query.GenerateSearchQuery();

            result.Options.Size.Should().Be(20);
        }
    }
}