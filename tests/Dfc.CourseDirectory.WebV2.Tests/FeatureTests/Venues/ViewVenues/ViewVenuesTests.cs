﻿using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using FluentAssertions.Execution;
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
                providerType: ProviderType.FE | ProviderType.TLevels);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/venues?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.Body.TextContent.Should().Contain("You have no venues");
        }

        [Fact]
        public async Task ViewVenues_Get_WithVenues_ReturnsExpectedContent()
        {
            //Arange
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.FE | ProviderType.TLevels);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/venues?providerId={provider.ProviderId}");

            var venues = await Task.WhenAll(Enumerable.Range(0, 3).Select(i => TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: $"TestVenue{i}")));

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            
            using (new AssertionScope())
            {
                foreach (var venue in venues)
                {
                    var venueRow = doc.GetElementByTestId($"venue-row-{venue.VenueId}");
                    venueRow.Should().NotBeNull();

                    venueRow.GetElementByTestId("venue-ref").TextContent.Should().Be(venue.ProviderVenueRef);
                    venueRow.GetElementByTestId("venue-name").TextContent.Should().Be(venue.VenueName);
                    venueRow.GetElementByTestId("venue-address").TextContent.Trim().Should().Be(string.Join("\n", new[] { venue.AddressLine1, venue.AddressLine2, venue.Town, venue.County, venue.Postcode }.Where(part => !string.IsNullOrWhiteSpace(part))));
                    venueRow.GetElementByTestId("venue-view-link").Attributes["href"].Value.Should().Be($"/venues/{venue.VenueId}");
                    venueRow.GetElementByTestId("venue-delete-link").Attributes["href"].Value.Should().Be($"/venues/{venue.VenueId}/delete");
                }
            }
        }
    }
}
