using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.Venues.EditVenue;
using FluentAssertions;
using FluentAssertions.Execution;
using FormFlow;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Venues.EditVenue
{
    public class AddressTests : MvcTestBase
    {
        public AddressTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                addressLine1: "Test Venue line 1",
                addressLine2: "Test Venue line 2",
                town: "Town",
                county: "County",
                postcode: "AB1 2DE")).VenueId;

            var request = new HttpRequestMessage(HttpMethod.Get, $"venues/{venueId}/address");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (new AssertionScope())
            {
                var doc = await response.GetDocument();
                doc.GetElementById("AddressLine1").GetAttribute("value").Should().Be("Test Venue line 1");
                doc.GetElementById("AddressLine2").GetAttribute("value").Should().Be("Test Venue line 2");
                doc.GetElementById("Town").GetAttribute("value").Should().Be("Town");
                doc.GetElementById("County").GetAttribute("value").Should().Be("County");
                doc.GetElementById("Postcode").GetAttribute("value").Should().Be("AB1 2DE");
            }
        }

        [Fact]
        public async Task Get_NewAddressAlreadySetInJourneyInstance_RendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;

            var journeyInstance = await CreateJourneyInstance(venueId);
            journeyInstance.UpdateState(state =>
            {
                state.AddressLine1 = "Updated line 1";
                state.AddressLine2 = "Updated line 2";
                state.Town = "Updated town";
                state.County = "Updated county";
                state.Postcode = "UP1 D8D";
            });

            var request = new HttpRequestMessage(HttpMethod.Get, $"venues/{venueId}/address");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (new AssertionScope())
            {
                var doc = await response.GetDocument();
                doc.GetElementById("AddressLine1").GetAttribute("value").Should().Be("Updated line 1");
                doc.GetElementById("AddressLine2").GetAttribute("value").Should().Be("Updated line 2");
                doc.GetElementById("Town").GetAttribute("value").Should().Be("Updated town");
                doc.GetElementById("County").GetAttribute("value").Should().Be("Updated county");
                doc.GetElementById("Postcode").GetAttribute("value").Should().Be("UP1 D8D");
            }
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Post_UserCannotAccessVenue_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;

            var anotherProvider = await TestData.CreateProvider();

            await TestData.CreatePostcodeInfo("CV1 2AA", latitude: 42D, longitude: 43D, inEngland: true);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("AddressLine1", "Updated address line 1")
                .Add("AddressLine2", "Updated address line 2")
                .Add("Town", "Updated town")
                .Add("County", "Updated county")
                .Add("Postcode", "CV1 2AA")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/address")
            {
                Content = requestContent
            };

            await User.AsTestUser(userType, anotherProvider.ProviderId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_VenueDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var venueId = Guid.NewGuid();

            await TestData.CreatePostcodeInfo("CV1 2AA", latitude: 42D, longitude: 43D, inEngland: true);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("AddressLine1", "Updated address line 1")
                .Add("AddressLine2", "Updated address line 2")
                .Add("Town", "Updated town")
                .Add("County", "Updated county")
                .Add("Postcode", "CV1 2AA")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/address")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Theory]
        [MemberData(nameof(InvalidAddressData))]
        public async Task Post_InvalidAddress_RendersError(
            string addressLine1,
            string addressLine2,
            string town,
            string county,
            string postcode,
            string expectedErrorInputId,
            string expectedErrorMessage)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;

            await TestData.CreatePostcodeInfo("CV1 2AA", latitude: 42D, longitude: 43D, inEngland: true);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("AddressLine1", addressLine1)
                .Add("AddressLine2", addressLine2)
                .Add("Town", town)
                .Add("County", county)
                .Add("Postcode", postcode)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/address")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError(expectedErrorInputId, expectedErrorMessage);
        }

        [Theory]
        [InlineData("England", false)]
        [InlineData("Scotland", true)]
        public async Task Post_ValidRequest_UpdatesJourneyInstanceAndRedirects(
            string onspdRecordCountry,
            bool expectedNewAddressIsOutsideOfEnglandValue)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;

            await TestData.CreatePostcodeInfo("CV1 2AA", latitude: 42D, longitude: 43D, onspdRecordCountry == "England");

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("AddressLine1", "Updated address line 1")
                .Add("AddressLine2", "Updated address line 2")
                .Add("Town", "Updated town")
                .Add("County", "Updated county")
                .Add("Postcode", "CV1 2AA")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/address")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.OriginalString.Should().Be($"/venues/{venueId}");

            using (new AssertionScope())
            {
                var journeyInstance = GetJourneyInstance(venueId);
                journeyInstance.State.AddressLine1.Should().Be("Updated address line 1");
                journeyInstance.State.AddressLine2.Should().Be("Updated address line 2");
                journeyInstance.State.Town.Should().Be("Updated town");
                journeyInstance.State.County.Should().Be("Updated county");
                journeyInstance.State.Postcode.Should().Be("CV1 2AA");
                journeyInstance.State.Latitude.Should().Be(42D);
                journeyInstance.State.Longitude.Should().Be(43D);
                journeyInstance.State.NewAddressIsOutsideOfEngland.Should().Be(expectedNewAddressIsOutsideOfEnglandValue);
            }
        }

        public static IEnumerable<object[]> InvalidAddressData { get; } =
            new[]
            {
                // Address line 1 missing
                (
                    AddressLine1: "",
                    AddressLine2: "Updated line 2",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "AddressLine1",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_ADDRESS_LINE1_REQUIRED"]
                ),
                // Address line 1 too long
                (
                    AddressLine1: new string('x', 101),
                    AddressLine2: "Updated line 2",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "AddressLine1",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_ADDRESS_LINE1_MAXLENGTH"]
                ),
                // Address line 1 has invalid characters
                (
                    AddressLine1: "!",
                    AddressLine2: "Updated line 2",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "AddressLine1",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_ADDRESS_LINE1_FORMAT"]
                ),
                // Address line 2 too long
                (
                    AddressLine1: "Updated line 1",
                    AddressLine2: new string('x', 101),
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "AddressLine2",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_ADDRESS_LINE2_MAXLENGTH"]
                ),
                // Address line 2 has invalid characters
                (
                    AddressLine1: "Updated line 1",
                    AddressLine2: "!",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "AddressLine2",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_ADDRESS_LINE2_FORMAT"]
                ),
                // Town missing
                (
                    AddressLine1: "",
                    AddressLine2: "Updated line 2",
                    Town: "",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "Town",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_TOWN_REQUIRED"]
                ),
                // Town is too long
                (
                    AddressLine1: "!",
                    AddressLine2: "Updated line 2",
                    Town: new string('x', 76),
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "Town",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_TOWN_MAXLENGTH"]
                ),
                // Town has invalid characters
                (
                    AddressLine1: "Updated line 1",
                    AddressLine2: "Updated line 2",
                    Town: "!",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "Town",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_TOWN_FORMAT"]
                ),
                // Postcode is not valid
                (
                    AddressLine1: "Updated line 1",
                    AddressLine2: "Updated line 2",
                    Town: "Updated town",
                    County: "Updated county",
                    Postcode: "X",
                    ExpectedErrorInputId: "Postcode",
                    ExpectedErrorMessage: ErrorRegistry.All["VENUE_POSTCODE_FORMAT"]
                )
            }
            .Select(t => new object[] { t.AddressLine1, t.AddressLine2, t.Town, t.County, t.Postcode, t.ExpectedErrorInputId, t.ExpectedErrorMessage })
            .ToArray();

        private async Task<JourneyInstance<EditVenueJourneyModel>> CreateJourneyInstance(Guid venueId)
        {
            var state = await WithSqlQueryDispatcher(async dispatcher =>
            {
                var modelFactory = CreateInstance<EditVenueJourneyModelFactory>(dispatcher);
                return await modelFactory.CreateModel(venueId);
            });

            return CreateJourneyInstance(
                journeyName: "EditVenue",
                configureKeys: keysBuilder => keysBuilder.With("venueId", venueId),
                state);
        }

        private JourneyInstance<EditVenueJourneyModel> GetJourneyInstance(Guid venueId) =>
            GetJourneyInstance<EditVenueJourneyModel>(
                journeyName: "EditVenue",
                configureKeys: keysBuilder => keysBuilder.With("venueId", venueId));
    }
}
