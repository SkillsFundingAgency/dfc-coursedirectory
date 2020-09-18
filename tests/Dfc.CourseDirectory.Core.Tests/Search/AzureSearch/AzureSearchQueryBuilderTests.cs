using System;
using System.Linq;
using Azure.Search.Documents.Models;
using Dfc.CourseDirectory.Core.Search.AzureSearch;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.Search.AzureSearch
{
    public class AzureSearchQueryBuilderTests
    {
        [Fact]
        public void Build_ReturnsExpectedQuery()
        {
            var searchFields = new[]
            {
                "TestSearchField1",
                "TestSearchField2",
                "TestSearchField3"
            };

            var facets = new[]
            {
                "TestFacet1",
                "TestFacet2",
                "TestFacet3"
            };

            var orderBy = new[]
            {
                "TestOrderBy1",
                "TestOrderBy2",
                "TestOrderBy3"
            };

            var builder = new AzureSearchQueryBuilder("TestSearchText")
                .WithSearchMode(SearchMode.All)
                .WithSearchFields(searchFields)
                .WithFacets(facets)
                .WithSearchInFilter("TestVariable1", new[] { "TestValue1" })
                .WithSearchInFilter("TestVariable2", new[] { "TestValue2", "TestValue3" })
                .WithSearchInFilter("TestVariable3", new[] { "TestValue4", "TestValue5", "TestValue6" }, ',')
                .WithOrderBy(orderBy)
                .WithScoringProfile("TestScoringProfile")
                .WithSize(12)
                .WithSkip(34)
                .WithIncludeTotalCount();

            var result = builder.Build();

            result.SearchText.Should().Be("TestSearchText");
            result.Options.SearchMode.Should().Be(SearchMode.All);
            result.Options.SearchFields.Should().Equal(searchFields);
            result.Options.Facets.Should().Equal(facets);
            result.Options.Filter.Should().Be("search.in(TestVariable1, 'TestValue1', '|') AND search.in(TestVariable2, 'TestValue2|TestValue3', '|') AND search.in(TestVariable3, 'TestValue4,TestValue5,TestValue6', ',')");
            result.Options.OrderBy.Should().Equal(orderBy);
            result.Options.ScoringProfile.Should().Be("TestScoringProfile");
            result.Options.Size.Should().Be(12);
            result.Options.Skip.Should().Be(34);
            result.Options.IncludeTotalCount.Should().BeTrue();
        }

        [Theory]
        [InlineData(null, "*")]
        [InlineData("", "*")]
        [InlineData(" ", "*")]
        [InlineData("*", "*")]
        [InlineData("TestSearchText", "TestSearchText")]
        public void Build_WithSearchText_ReturnsQueryWithExpectedSearchText(string searchText, string expectedResult)
        {
            var builder = new AzureSearchQueryBuilder(searchText);

            var result = builder.Build();

            result.SearchText.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(SearchMode.All)]
        [InlineData(SearchMode.Any)]
        public void Build_WithSearchMode_ReturnsQueryWithExpectedSearchMode(SearchMode? searchMode)
        {
            var builder = new AzureSearchQueryBuilder("TestSearchText")
                .WithSearchMode(searchMode);

            var result = builder.Build();

            result.Options.SearchMode.Should().Be(searchMode);
        }

        [Fact]
        public void Build_WithSearchFields_ReturnsQueryWithExpectedSearchFields()
        {
            var searchFields = new[]
            {
                "TestSearchField1",
                "TestSearchField2",
                "TestSearchField3"
            };

            var builder = new AzureSearchQueryBuilder("TestSearchText")
                .WithSearchFields(searchFields);

            var result = builder.Build();

            result.Options.SearchFields.Should().Equal(searchFields);
        }

        [Fact]
        public void Build_WithFacets_ReturnsQueryWithExpectedFacets()
        {
            var facets = new[]
            {
                "TestFacet1",
                "TestFacet2",
                "TestFacet3"
            };

            var builder = new AzureSearchQueryBuilder("TestSearchText")
                .WithFacets(facets);

            var result = builder.Build();

            result.Options.Facets.Should().Equal(facets);
        }

        [Fact]
        public void Build_WithSingleSearchInFilterAndSingleValue_ReturnsQueryWithExpectedFilter()
        {
            var builder = new AzureSearchQueryBuilder("TestSearchText")
                .WithSearchInFilter("TestVariable1", new[] { "TestValue1" });

            var result = builder.Build();

            result.Options.Filter.Should().Be("search.in(TestVariable1, 'TestValue1', '|')");
        }

        [Fact]
        public void Build_WithSingleSearchInFilterAndMultipleValues_ReturnsQueryWithExpectedFilter()
        {
            var builder = new AzureSearchQueryBuilder("TestSearchText")
                .WithSearchInFilter("TestVariable1", new[] { "TestValue1", "TestValue2", "TestValue3" });

            var result = builder.Build();

            result.Options.Filter.Should().Be("search.in(TestVariable1, 'TestValue1|TestValue2|TestValue3', '|')");
        }

        [Fact]
        public void Build_WithMultipleSearchInFilters_ReturnsQueryWithExpectedFilter()
        {
            var builder = new AzureSearchQueryBuilder("TestSearchText")
                .WithSearchInFilter("TestVariable1", new[] { "TestValue1" })
                .WithSearchInFilter("TestVariable2", new[] { "TestValue2", "TestValue3" });

            var result = builder.Build();

            result.Options.Filter.Should().Be("search.in(TestVariable1, 'TestValue1', '|') AND search.in(TestVariable2, 'TestValue2|TestValue3', '|')");
        }

        [Fact]
        public void Build_WithDelimiter_ReturnsQueryWithExpectedFilter()
        {
            var builder = new AzureSearchQueryBuilder("TestSearchText")
                .WithSearchInFilter("TestVariable1", new[] { "TestValue1", "TestValue2", "TestValue3" }, ',');

            var result = builder.Build();

            result.Options.Filter.Should().Be("search.in(TestVariable1, 'TestValue1,TestValue2,TestValue3', ',')");
        }

        [Fact]
        public void Build_WithOrderBy_ReturnsQueryWithExpectedOrderBy()
        {
            var orderBy = new[]
            {
                "TestOrderBy1",
                "TestOrderBy2",
                "TestOrderBy3"
            };

            var builder = new AzureSearchQueryBuilder("TestSearchText")
                .WithOrderBy(orderBy);

            var result = builder.Build();

            result.Options.OrderBy.Should().Equal(orderBy);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("TestScoringProfile")]
        public void Build_WithScoringProfile_ReturnsQueryWithExpectedScoringProfile(string scoringProfile)
        {
            var builder = new AzureSearchQueryBuilder("TestSearchText")
                .WithScoringProfile(scoringProfile);

            var result = builder.Build();

            result.Options.ScoringProfile.Should().Be(scoringProfile);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(123)]
        public void Build_WithSize_ReturnsQueryWithExpectedSize(int? size)
        {
            var builder = new AzureSearchQueryBuilder("TestSearchText")
                .WithSize(size);

            var result = builder.Build();

            result.Options.Size.Should().Be(size);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(123)]
        public void Build_WithSkip_ReturnsQueryWithExpectedSkip(int? skip)
        {
            var builder = new AzureSearchQueryBuilder("TestSearchText")
                .WithSkip(skip);

            var result = builder.Build();

            result.Options.Skip.Should().Be(skip);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(true)]
        [InlineData(false)]
        public void Build_WithInvludeTotalCount_ReturnsQueryWithExpectedTotalCount(bool? includeTotalCount)
        {
            var builder = new AzureSearchQueryBuilder("TestSearchText")
                .WithIncludeTotalCount(includeTotalCount);

            var result = builder.Build();

            result.Options.IncludeTotalCount.Should().Be(includeTotalCount);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void WithSearchInFilter_WithNullOrWhiteSpaceVariable_ThrowsException(string variable)
        {
            var builder = new AzureSearchQueryBuilder("TestSearchText");

            Func<AzureSearchQueryBuilder> action = () => builder.WithSearchInFilter(variable, new[] { "TestValue1", "TestValue2", "TestValue3" });

            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void WithSearchInFilter_WithValuesContainsDelimiter_ThrowsException()
        {
            var builder = new AzureSearchQueryBuilder("TestSearchText");

            Func<AzureSearchQueryBuilder> action = () => builder.WithSearchInFilter("TestVariable1", new[] { "TestValue1", "Test|Value2", "TestValue3" });

            action.Should().ThrowExactly<ArgumentException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void WithSize_WithInvalidSize_ThrowsException(int? size)
        {
            var builder = new AzureSearchQueryBuilder("TestSearchText");

            Func<AzureSearchQueryBuilder> action = () => builder.WithSize(size);

            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void WithSize_WithInvalidSkip_ThrowsException(int? skip)
        {
            var builder = new AzureSearchQueryBuilder("TestSearchText");

            Func<AzureSearchQueryBuilder> action = () => builder.WithSkip(skip);

            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }
    }
}