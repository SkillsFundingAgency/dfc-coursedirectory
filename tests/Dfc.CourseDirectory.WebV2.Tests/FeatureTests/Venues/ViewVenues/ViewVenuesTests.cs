using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Venues.ViewVenues
{
    public class ViewVenuesTests : MvcTestBase
    {
        public ViewVenuesTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task ViewVenues_Get_WithNoVenues_ReturnsExpectedContent()
        {
            //Arange
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.FE | ProviderType.Apprenticeships | ProviderType.TLevels);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/venues?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var doc = await response.GetDocument();
            doc.Body.TextContent.Should().Contain("You have no locations");
        }

        [Fact]
        public async Task ViewVenues_Get_WithVenues_ReturnsExpectedContent()
        {
            //Arange
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.FE | ProviderType.Apprenticeships | ProviderType.TLevels);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/venues?providerId={provider.ProviderId}");

            var venues = await Task.WhenAll(Enumerable.Range(0, 3).Select(i => TestData.CreateVenue(provider.ProviderId, venueName: $"TestVenue{i}")));

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var doc = await response.GetDocument();
            
            using (new AssertionScope())
            {
                foreach (var venue in venues)
                {
                    var venueRow = doc.GetElementByTestId($"venue-row-{venue.Id}");
                    venueRow.Should().NotBeNull();

                    venueRow.GetElementByTestId("venue-name").TextContent.Should().Be(venue.VenueName);
                    venueRow.GetElementByTestId("venue-address").TextContent.Trim().Should().Be(string.Join(Environment.NewLine, new[] { venue.AddressLine1, venue.AddressLine2, venue.Town, venue.County }.Where(part => !string.IsNullOrWhiteSpace(part))));
                    venueRow.GetElementByTestId("venue-postcode").TextContent.Should().Be(venue.Postcode);
                    venueRow.GetElementByTestId("venue-view-link").Attributes["href"].Value.Should().Be($"/venues/{venue.Id}");
                    venueRow.GetElementByTestId("venue-delete-link").Attributes["href"].Value.Should().Be($"/venues/{venue.Id}/delete");
                }
            }
        }
    }
}
