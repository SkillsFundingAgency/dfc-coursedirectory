using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.Venues.AddVenue;
using FluentAssertions;
using FluentAssertions.Execution;
using FormFlow;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Venues.AddVenue
{
    public class DetailsTests : MvcTestBase
    {
        public DetailsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_ValidRequest_ReturnsExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var name = "My Venue";
            var email = "email@example.com";
            var telephone = "020 7946 0000";
            var website = "example.com";

            var journeyInstance = CreateJourneyInstance(
                provider.ProviderId,
                new AddVenueJourneyModel()
                {
                    Name = name,
                    Email = email,
                    Telephone = telephone,
                    Website = website,
                    ValidStages = AddVenueCompletedStages.Address
                });

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/venues/add/details?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (new AssertionScope())
            {
                var doc = await response.GetDocument();
                doc.GetElementById("Name").GetAttribute("value").Should().Be(name);
                doc.GetElementById("Email").GetAttribute("value").Should().Be(email);
                doc.GetElementById("Telephone").GetAttribute("value").Should().Be(telephone);
                doc.GetElementById("Website").GetAttribute("value").Should().Be(website);
            }
        }

        [Theory]
        [MemberData(nameof(InvalidAddressData))]
        public async Task Post_InvalidDetails_RendersErrorMessage(
            string name,
            string email,
            string telephone,
            string website,
            string expectedErrorInputId,
            string expectedErrorCode)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            // Create another venue for the provider for testing the unique venue name error case
            await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: "Existing Venue");

            var journeyInstance = CreateJourneyInstance(
                provider.ProviderId,
                new AddVenueJourneyModel()
                {
                    ValidStages = AddVenueCompletedStages.Address
                });

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/venues/add/details?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("Name", name)
                    .Add("Email", email)
                    .Add("Telephone", telephone)
                    .Add("Website", website)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasErrorWithCode(expectedErrorInputId, expectedErrorCode);
        }

        [Fact]
        public async Task Post_ValidRequest_UpdatesJourneyStateAndRedirects()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var name = "My Venue";
            var email = "email@example.com";
            var telephone = "020 7946 0000";
            var website = "example.com";

            var journeyInstance = CreateJourneyInstance(
                provider.ProviderId,
                new AddVenueJourneyModel()
                {
                    ValidStages = AddVenueCompletedStages.Address
                });

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/venues/add/details?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("Name", name)
                    .Add("Email", email)
                    .Add("Telephone", telephone)
                    .Add("Website", website)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);

            response.Headers.Location.OriginalString.Should()
                .Be($"/venues/add/check-publish?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}");

            using (new AssertionScope())
            {
                journeyInstance = GetJourneyInstance<AddVenueJourneyModel>(journeyInstance.InstanceId);
                journeyInstance.State.Name.Should().Be(name);
                journeyInstance.State.Email.Should().Be(email);
                journeyInstance.State.Telephone.Should().Be(telephone);
                journeyInstance.State.Website.Should().Be(website);
            }
        }

        public static IEnumerable<object[]> InvalidAddressData { get; } =
            new[]
            {
                // Name is not unique for provider
                (
                    Name: "Existing Venue",
                    Email: "",
                    Telephone: "",
                    Website: "",
                    ExpectedErrorInputId: "Name",
                    ExpectedErrorCode: "VENUE_NAME_UNIQUE"
                ),
                // Invalid Email
                (
                    Name: "My Venue",
                    Email: "xxx",
                    Telephone: "",
                    Website: "",
                    ExpectedErrorInputId: "Email",
                    ExpectedErrorCode: "VENUE_EMAIL_FORMAT"
                ),
                // Invalid PhoneNumber
                (
                    Name: "My Venue",
                    Email: "",
                    Telephone: "xxx",
                    Website: "",
                    ExpectedErrorInputId: "Telephone",
                    ExpectedErrorCode: "VENUE_TELEPHONE_FORMAT"
                ),
                // Invalid Website
                (
                    Name: "My Venue",
                    Email: "",
                    Telephone: "",
                    Website: ":bad/website",
                    ExpectedErrorInputId: "Website",
                    ExpectedErrorCode: "VENUE_WEBSITE_FORMAT"
                )
            }
            .Select(t => new object[] { t.Name, t.Email, t.Telephone, t.Website, t.ExpectedErrorInputId, t.ExpectedErrorCode })
            .ToArray();

        private JourneyInstance<AddVenueJourneyModel> CreateJourneyInstance(Guid providerId, AddVenueJourneyModel state = null)
        {
            state ??= new AddVenueJourneyModel();

            var uniqueKey = Guid.NewGuid().ToString();

            return CreateJourneyInstance(
                journeyName: "AddVenue",
                configureKeys: keysBuilder => keysBuilder.With("providerId", providerId),
                state,
                uniqueKey: uniqueKey);
        }
    }
}
