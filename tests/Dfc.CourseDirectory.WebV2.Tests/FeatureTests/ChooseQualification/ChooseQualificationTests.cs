using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Search.Documents.Models;
using Dfc.CourseDirectory.Core.Models;
using dfc = Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.Apprenticeships.ClassroomLocation;
using FluentAssertions;
using Moq;
using Xunit;
using FluentAssertions.Execution;

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

        //Filter count is correct
        //Error message is displayed when search term is incorrect
    }
}



