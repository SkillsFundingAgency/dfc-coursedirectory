using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.AzureSearch;
using FluentAssertions;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.Search.AzureSearch
{
    public class AzureSearchClientTests
    {
        private readonly Mock<SearchClient> _searchClient;
        private readonly AzureSearchClient<object> _client;

        public AzureSearchClientTests()
        {
            _searchClient = new Mock<SearchClient>();
            _client = new AzureSearchClient<object>(_searchClient.Object);
        }

        [Fact]
        public async Task Search_WithNullQuery_ThrowsException()
        {
            Func<Task<Core.Search.SearchResult<object>>> action = () => _client.Search(null as IAzureSearchQuery<object>);
            await action.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task Search_WithQueryNotImplementsIAzureSearchQuery_ThrowsException()
        {
            Func<Task<Core.Search.SearchResult<object>>> action = () => _client.Search(new Mock<ISearchQuery<object>>().Object);
            await action.Should().ThrowExactlyAsync<ArgumentException>();
        }

        [Fact]
        public async Task Search_GeneratesQuery_ReturnsExpectedResults()
        {
            var capturedSearchText = default(string);
            var capturedSearchOptions = default(SearchOptions);

            var searchResults = SearchModelFactory.SearchResults(
                new[]
                {
                    SearchModelFactory.SearchResult<object>("TestResult1", 1.23, null),
                    SearchModelFactory.SearchResult<object>("TestResult2", 2.34, null),
                    SearchModelFactory.SearchResult<object>("TestResult3", 3.45, null)
                },
                3,
                new Dictionary<string, IList<FacetResult>>
                {
                    {
                        "TestFacet1",
                        new List<FacetResult>
                        {
                            SearchModelFactory.FacetResult(12, new Dictionary<string, object> { { "value", "TestFacetValue1" } }),
                            SearchModelFactory.FacetResult(34, new Dictionary<string, object> { { "value", "TestFacetValue2" } }),
                            SearchModelFactory.FacetResult(56, new Dictionary<string, object> { { "value", "TestFacetValue3" } })
                        }
                    },
                    {
                        "TestFacet2",
                        new List<FacetResult>
                        {
                            SearchModelFactory.FacetResult(78, new Dictionary<string, object> { { "value", "TestFacetValue4" } }),
                            SearchModelFactory.FacetResult(90, new Dictionary<string, object> { { "value", "TestFacetValue5" } })
                        }
                    }
                },
                null,
                null);

            var response = new Mock<Response<SearchResults<object>>>();
            response.Setup(s => s.Value)
                .Returns(searchResults);

            _searchClient.Setup(s => s.SearchAsync<object>(It.IsAny<string>(), It.IsAny<SearchOptions>(), It.IsAny<CancellationToken>()))
                .Callback<string, SearchOptions, CancellationToken>((s, o, ct) =>
                {
                    capturedSearchText = s;
                    capturedSearchOptions = o;
                })
                .ReturnsAsync(response.Object);

            var searchText = "TestSearchText";
            var searchOptions = new SearchOptions();

            var query = new Mock<IAzureSearchQuery<object>>();
            query.Setup(s => s.GenerateSearchQuery())
                .Returns((searchText, searchOptions));

            var result = await _client.Search(query.Object);

            capturedSearchText.Should().Be(searchText);
            capturedSearchOptions.Should().Be(searchOptions);

            result.Should().NotBeNull();
            result.Results.Should().BeEquivalentTo(new[] { "TestResult1", "TestResult2", "TestResult3" });
            result.Facets.Should().BeEquivalentTo(new Dictionary<string, Dictionary<string, long?>>
            {
                {
                    "TestFacet1",
                    new Dictionary<string, long?>
                    {
                        {  "TestFacetValue1", 12 },
                        {  "TestFacetValue2", 34 },
                        {  "TestFacetValue3", 56 }
                    }
                },
                {
                    "TestFacet2",
                    new Dictionary<string, long?>
                    {
                        {  "TestFacetValue4", 78 },
                        {  "TestFacetValue5", 90 }
                    }
                }
            });
            result.TotalCount.Should().Be(3);
        }
    }
}