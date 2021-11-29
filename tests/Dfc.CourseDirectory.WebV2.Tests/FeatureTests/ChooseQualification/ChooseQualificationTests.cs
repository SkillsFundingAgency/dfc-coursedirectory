using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Search.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;
using dfc = Dfc.CourseDirectory.Core.Search;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ChooseQualification
{
    public class ChooseQualification : MvcTestBase
    {
        public ChooseQualification(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        private async Task ChooseQualification_ResultCountIsCorrect()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var facets = new Dictionary<object, long?> { { new Object(), 343 } };
            dfc.SearchResult<Lars> res = new dfc.SearchResult<Lars>
            {
                Items = new ReadOnlyCollection<dfc.SearchResultItem<Lars>>(new List<dfc.SearchResultItem<Lars>>()
                {
                    new dfc.SearchResultItem<Lars>()
                    {
                        Record = new Lars(),
                    },
                    new dfc.SearchResultItem<Lars>()
                    {
                        Record = new Lars(),
                    }
                }),
                TotalCount = 2,
                Facets = new Dictionary<string, IReadOnlyDictionary<object, long?>>
                {
                    { "AwardOrgCode", facets },
                    { "NotionalNVQLevelv2", facets }
                }
            };
            LarsSearchClient.Setup(x => x.Search(It.IsAny<dfc.LarsSearchQuery>())).ReturnsAsync(res);
            LarsSearchSettings.Setup(x => x.Value).Returns(new Core.Configuration.LarsSearchSettings() { ItemsPerPage = 20 });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/courses/choose-qualification/search?SearchTerm=test&providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementByTestId("resultscount").TextContent.Should().Be("2");
            }
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        private async Task ChooseQualification_NotionalNVQLevelv2FiltersCount_IsCorrect()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var notionalNVQLevelv2Facets = new Dictionary<object, long?> { { new Object(), 343 }, { new Object(), 3433 }, { new Object(), 341 } };
            var awardOrgCodeFacets = new Dictionary<object, long?>();
            dfc.SearchResult<Lars> res = new dfc.SearchResult<Lars>
            {
                Items = new ReadOnlyCollection<dfc.SearchResultItem<Lars>>(new List<dfc.SearchResultItem<Lars>>()
                {
                    new dfc.SearchResultItem<Lars>()
                    {
                        Record = new Lars(),
                    },
                    new dfc.SearchResultItem<Lars>()
                    {
                        Record = new Lars(),
                    }
                }),
                TotalCount = 2,
                Facets = new Dictionary<string, IReadOnlyDictionary<object, long?>>
                {
                    { "AwardOrgCode", awardOrgCodeFacets },
                    { "NotionalNVQLevelv2", notionalNVQLevelv2Facets }
                }
            };
            LarsSearchClient.Setup(x => x.Search(It.IsAny<dfc.LarsSearchQuery>())).ReturnsAsync(res);
            LarsSearchSettings.Setup(x => x.Value).Returns(new Core.Configuration.LarsSearchSettings() { ItemsPerPage = 20 });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/courses/choose-qualification/search?SearchTerm=test&providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementsByClassName("NotionalNVQLevelv2FilterOption").Length.Should().Be(3);
                doc.GetElementsByClassName("AwardingOrganisationFilterOption").Length.Should().Be(0);
            }
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        private async Task ChooseQualification_AwardingOrganisationFilterOptionCount_IsCorrect()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var notionalNVQLevelv2Facets = new Dictionary<object, long?>();
            var awardOrgCodeFacets = new Dictionary<object, long?> { { new Object(), 343 }, { new Object(), 3433 } };
            dfc.SearchResult<Lars> res = new dfc.SearchResult<Lars>
            {
                Items = new ReadOnlyCollection<dfc.SearchResultItem<Lars>>(new List<dfc.SearchResultItem<Lars>>()
                {
                    new dfc.SearchResultItem<Lars>()
                    {
                        Record = new Lars(),
                    },
                    new dfc.SearchResultItem<Lars>()
                    {
                        Record = new Lars(),
                    }
                }),
                TotalCount = 2,
                Facets = new Dictionary<string, IReadOnlyDictionary<object, long?>>
                {
                    { "AwardOrgCode", awardOrgCodeFacets },
                    { "NotionalNVQLevelv2", notionalNVQLevelv2Facets }
                }
            };
            LarsSearchClient.Setup(x => x.Search(It.IsAny<dfc.LarsSearchQuery>())).ReturnsAsync(res);
            LarsSearchSettings.Setup(x => x.Value).Returns(new Core.Configuration.LarsSearchSettings() { ItemsPerPage = 20 });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/courses/choose-qualification/search?SearchTerm=test&providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementsByClassName("NotionalNVQLevelv2FilterOption").Length.Should().Be(0);
                doc.GetElementsByClassName("AwardingOrganisationFilterOption").Length.Should().Be(2);
            }
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("")]
        [InlineData("f")]
        [InlineData("ff")]
        private async Task ChooseQualification_SearchTermLengthInvalid_RendersError(string term)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var notionalNVQLevelv2Facets = new Dictionary<object, long?>();
            var awardOrgCodeFacets = new Dictionary<object, long?> { { new Object(), 343 }, { new Object(), 3433 } };
            dfc.SearchResult<Lars> res = new dfc.SearchResult<Lars>
            {
                Items = new ReadOnlyCollection<dfc.SearchResultItem<Lars>>(new List<dfc.SearchResultItem<Lars>>()
                {
                    new dfc.SearchResultItem<Lars>()
                    {
                        Record = new Lars(),
                    },
                    new dfc.SearchResultItem<Lars>()
                    {
                        Record = new Lars(),
                    }
                }),
                TotalCount = 2,
                Facets = new Dictionary<string, IReadOnlyDictionary<object, long?>>
                {
                    { "AwardOrgCode", awardOrgCodeFacets },
                    { "NotionalNVQLevelv2", notionalNVQLevelv2Facets }
                }
            };
            LarsSearchClient.Setup(x => x.Search(It.IsAny<dfc.LarsSearchQuery>())).ReturnsAsync(res);
            LarsSearchSettings.Setup(x => x.Value).Returns(new Core.Configuration.LarsSearchSettings() { ItemsPerPage = 20 });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/courses/choose-qualification/search?SearchTerm={term}&providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementByTestId("SearchErrorMessage").TextContent.Should().Be("Enter search criteria");
            }
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        private async Task ChooseQualification_FirstEntryToPage_DoesNotRenderFilters()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var notionalNVQLevelv2Facets = new Dictionary<object, long?>();
            var awardOrgCodeFacets = new Dictionary<object, long?> { { new Object(), 343 }, { new Object(), 3433 } };
            dfc.SearchResult<Lars> res = new dfc.SearchResult<Lars>
            {
                Items = new ReadOnlyCollection<dfc.SearchResultItem<Lars>>(new List<dfc.SearchResultItem<Lars>>()
                {
                    new dfc.SearchResultItem<Lars>()
                    {
                        Record = new Lars(),
                    },
                    new dfc.SearchResultItem<Lars>()
                    {
                        Record = new Lars(),
                    }
                }),
                TotalCount = 2,
                Facets = new Dictionary<string, IReadOnlyDictionary<object, long?>>
                {
                    { "AwardOrgCode", awardOrgCodeFacets },
                    { "NotionalNVQLevelv2", notionalNVQLevelv2Facets }
                }
            };
            LarsSearchClient.Setup(x => x.Search(It.IsAny<dfc.LarsSearchQuery>())).ReturnsAsync(res);
            LarsSearchSettings.Setup(x => x.Value).Returns(new Core.Configuration.LarsSearchSettings() { ItemsPerPage = 20 });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/courses/choose-qualification/?providerId={provider.ProviderId}");
            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementByTestId("NotionalNVQLevelv2Filters").Should().BeNull();
                doc.GetElementByTestId("AwardingOrganisationFilters").Should().BeNull();
            }
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}



