using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.Venues.AddVenue;
using FluentAssertions;
using FluentAssertions.Execution;
using FormFlow;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Venues.AddVenue
{
    public class AddressTests : MvcTestBase
    {
        public AddressTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_JourneyIsCompleted_ReturnsConflict()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var addressLine1 = "Test Venue line 1";
            var addressLine2 = "Test Venue line 2";
            var town = "Town";
            var county = "County";
            var postcode = "AB1 2DE";

            var journeyInstance = CreateJourneyInstance(
                provider.ProviderId,
                new AddVenueJourneyModel()
                {
                    AddressLine1 = addressLine1,
                    AddressLine2 = addressLine2,
                    Town = town,
                    County = county,
                    Postcode = postcode
                });

            GetJourneyInstance<AddVenueJourneyModel>(journeyInstance.InstanceId).Complete();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/venues/add/address?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("ab12de", "AB1 2DE")]
        [InlineData("xxx", "xxx")]
        public async Task Get_ValidRequestWithNoExistingAddressInState_RendersExpectedOutput(
            string requestPostcode,
            string expectedPostcodeInputValue)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var journeyInstance = CreateJourneyInstance(provider.ProviderId);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/venues/add/address?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}&postcode={requestPostcode}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            using (new AssertionScope())
            {
                var doc = await response.GetDocument();
                doc.GetElementById("AddressLine1").GetAttribute("value").Should().BeNullOrEmpty();
                doc.GetElementById("AddressLine2").GetAttribute("value").Should().BeNullOrEmpty();
                doc.GetElementById("Town").GetAttribute("value").Should().BeNullOrEmpty();
                doc.GetElementById("County").GetAttribute("value").Should().BeNullOrEmpty();
                doc.GetElementById("Postcode").GetAttribute("value").Should().Be(expectedPostcodeInputValue);
            }
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var addressLine1 = "Test Venue line 1";
            var addressLine2 = "Test Venue line 2";
            var town = "Town";
            var county = "County";
            var postcode = "AB1 2DE";

            var journeyInstance = CreateJourneyInstance(
                provider.ProviderId,
                new AddVenueJourneyModel()
                {
                    AddressLine1 = addressLine1,
                    AddressLine2 = addressLine2,
                    Town = town,
                    County = county,
                    Postcode = postcode
                });

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/venues/add/address?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            using (new AssertionScope())
            {
                var doc = await response.GetDocument();
                doc.GetElementById("AddressLine1").GetAttribute("value").Should().Be(addressLine1);
                doc.GetElementById("AddressLine2").GetAttribute("value").Should().Be(addressLine2);
                doc.GetElementById("Town").GetAttribute("value").Should().Be(town);
                doc.GetElementById("County").GetAttribute("value").Should().Be(county);
                doc.GetElementById("Postcode").GetAttribute("value").Should().Be(postcode);
            }
        }

        [Fact]
        public async Task Post_JourneyIsCompleted_ReturnsConflict()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var addressLine1 = "Test Venue line 1";
            var addressLine2 = "Test Venue line 2";
            var town = "Town";
            var county = "County";
            var country = "England";
            var postcode = "AB1 2DE";

            var postcodeLatitude = 42D;
            var postcodeLongitude = 43D;

            await TestData.CreatePostcodeInfo(postcode, postcodeLatitude, postcodeLongitude, country == "England");

            var journeyInstance = CreateJourneyInstance(provider.ProviderId);

            GetJourneyInstance<AddVenueJourneyModel>(journeyInstance.InstanceId).Complete();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/venues/add/address?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("AddressLine1", addressLine1)
                    .Add("AddressLine2", addressLine2)
                    .Add("Town", town)
                    .Add("County", county)
                    .Add("Postcode", postcode)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
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

            var postcodeLatitude = 42D;
            var postcodeLongitude = 43D;

            await TestData.CreatePostcodeInfo(postcode, postcodeLatitude, postcodeLongitude, inEngland: true);
            
            var journeyInstance = CreateJourneyInstance(provider.ProviderId);

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/venues/add/address?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("AddressLine1", addressLine1)
                    .Add("AddressLine2", addressLine2)
                    .Add("Town", town)
                    .Add("County", county)
                    .Add("Postcode", postcode)
                    .ToContent()
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

            var addressLine1 = "Test Venue line 1";
            var addressLine2 = "Test Venue line 2";
            var town = "Town";
            var county = "County";
            var postcode = "AB1 2DE";

            var postcodeLatitude = 42D;
            var postcodeLongitude = 43D;

            await TestData.CreatePostcodeInfo(postcode, postcodeLatitude, postcodeLongitude, onspdRecordCountry == "England");

            var journeyInstance = CreateJourneyInstance(provider.ProviderId);

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/venues/add/address?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("AddressLine1", addressLine1)
                    .Add("AddressLine2", addressLine2)
                    .Add("Town", town)
                    .Add("County", county)
                    .Add("Postcode", postcode)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);

            response.Headers.Location.OriginalString.Should()
                .Be($"/venues/add/details?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}");

            using (new AssertionScope())
            {
                journeyInstance = GetJourneyInstance<AddVenueJourneyModel>(journeyInstance.InstanceId);
                journeyInstance.State.AddressLine1.Should().Be(addressLine1);
                journeyInstance.State.AddressLine2.Should().Be(addressLine2);
                journeyInstance.State.Town.Should().Be(town);
                journeyInstance.State.County.Should().Be(county);
                journeyInstance.State.Postcode.Should().Be(postcode);
                journeyInstance.State.Latitude.Should().Be(postcodeLatitude);
                journeyInstance.State.Longitude.Should().Be(postcodeLongitude);
                journeyInstance.State.AddressIsOutsideOfEngland.Should().Be(expectedNewAddressIsOutsideOfEnglandValue);
            }
        }

        [Fact]
        public async Task Post_ValidRequest_NormalizesPostcode()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var inputPostcode = "ab12de";

            var addressLine1 = "Test Venue line 1";
            var addressLine2 = "Test Venue line 2";
            var town = "Town";
            var county = "County";
            var country = "England";
            var normalizedPostcode = "AB1 2DE";

            var postcodeLatitude = 42D;
            var postcodeLongitude = 43D;

            await TestData.CreatePostcodeInfo(normalizedPostcode, postcodeLatitude, postcodeLongitude, country == "England");

            var journeyInstance = CreateJourneyInstance(provider.ProviderId);

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/venues/add/address?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("AddressLine1", addressLine1)
                    .Add("AddressLine2", addressLine2)
                    .Add("Town", town)
                    .Add("County", county)
                    .Add("Postcode", inputPostcode)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);

            response.Headers.Location.OriginalString.Should()
                .Be($"/venues/add/details?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}");

            journeyInstance = GetJourneyInstance<AddVenueJourneyModel>(journeyInstance.InstanceId);
            journeyInstance.State.Postcode.Should().Be(normalizedPostcode);
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
                    ExpectedErrorMessage: "Enter address line 1"
                ),
                // Address line 1 too long
                (
                    AddressLine1: new string('x', 101),
                    AddressLine2: "Updated line 2",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "AddressLine1",
                    ExpectedErrorMessage: "Address line 1 must be 100 characters or less"
                ),
                // Address line 1 has invalid characters
                (
                    AddressLine1: "!",
                    AddressLine2: "Updated line 2",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "AddressLine1",
                    ExpectedErrorMessage: "Address line 1 must only include letters a to z, numbers, hyphens and spaces"
                ),
                // Address line 2 too long
                (
                    AddressLine1: "Updated line 1",
                    AddressLine2: new string('x', 101),
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "AddressLine2",
                    ExpectedErrorMessage: "Address line 2 must be 100 characters or less"
                ),
                // Address line 2 has invalid characters
                (
                    AddressLine1: "Updated line 1",
                    AddressLine2: "!",
                    Town: "Updated county",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "AddressLine2",
                    ExpectedErrorMessage: "Address line 2 must only include letters a to z, numbers, hyphens and spaces"
                ),
                // Town missing
                (
                    AddressLine1: "",
                    AddressLine2: "Updated line 2",
                    Town: "",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "Town",
                    ExpectedErrorMessage: "Enter a town or city"
                ),
                // Town is too long
                (
                    AddressLine1: "!",
                    AddressLine2: "Updated line 2",
                    Town: new string('x', 76),
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "Town",
                    ExpectedErrorMessage: "Town or city must be 75 characters or less"
                ),
                // Town has invalid characters
                (
                    AddressLine1: "Updated line 1",
                    AddressLine2: "Updated line 2",
                    Town: "!",
                    County: "Updated county",
                    Postcode: "CV1 2AA",
                    ExpectedErrorInputId: "Town",
                    ExpectedErrorMessage: "Town or city must only include letters a to z, numbers, hyphens and spaces"
                ),
                // Postcode is not valid
                (
                    AddressLine1: "Updated line 1",
                    AddressLine2: "Updated line 2",
                    Town: "Updated town",
                    County: "Updated county",
                    Postcode: "X",
                    ExpectedErrorInputId: "Postcode",
                    ExpectedErrorMessage: "Enter a real postcode"
                )
            }
            .Select(t => new object[] { t.AddressLine1, t.AddressLine2, t.Town, t.County, t.Postcode, t.ExpectedErrorInputId, t.ExpectedErrorMessage })
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
