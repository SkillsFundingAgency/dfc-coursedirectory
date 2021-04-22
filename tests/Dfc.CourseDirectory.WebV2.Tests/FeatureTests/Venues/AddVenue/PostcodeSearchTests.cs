using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.AddressSearch;
using Dfc.CourseDirectory.WebV2.Features.Venues.AddVenue;
using FluentAssertions;
using FluentAssertions.Execution;
using FormFlow;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Venues.AddVenue
{
    public class PostcodeSearchTests : MvcTestBase
    {
        public PostcodeSearchTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task PostcodeSearch_InvalidPostcode_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var postcode = "xxx";

            var journeyInstance = CreateJourneyInstance(provider.ProviderId);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/venues/add/postcode-search?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}&postcode={postcode}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("Postcode", "Enter a real postcode");
        }

        [Fact]
        public async Task PostcodeSearch_PostcodeNotInOnspdData_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var town = "Town";
            var postcode = "AB1 2DE";

            ConfigureAddressSearchServiceResults(postcode, town, resultCount: 3);

            var journeyInstance = CreateJourneyInstance(provider.ProviderId);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/venues/add/postcode-search?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}&postcode={postcode}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("Postcode", "Enter a real postcode");
        }

        [Fact]
        public async Task PostcodeSearch_SearchReturnsZeroResults_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var town = "Town";
            var country = "England";
            var postcode = "AB1 2DE";

            var postcodeLatitude = 42D;
            var postcodeLongitude = 43D;

            await TestData.CreatePostcodeInfo(postcode, postcodeLatitude, postcodeLongitude, country == "England");

            ConfigureAddressSearchServiceResults(postcode, town, resultCount: 0);

            var journeyInstance = CreateJourneyInstance(provider.ProviderId);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/venues/add/postcode-search?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}&postcode={postcode}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("Postcode", "Enter a real postcode");
        }

        [Fact]
        public async Task PostcodeSearch_ValidRequest_RendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var town = "Town";
            var country = "England";
            var postcode = "AB1 2DE";

            var postcodeLatitude = 42D;
            var postcodeLongitude = 43D;

            await TestData.CreatePostcodeInfo(postcode, postcodeLatitude, postcodeLongitude, country == "England");

            ConfigureAddressSearchServiceResults(postcode, town, resultCount: 3);

            var journeyInstance = CreateJourneyInstance(provider.ProviderId);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/venues/add/postcode-search?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}&postcode={postcode}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (new AssertionScope())
            {
                var doc = await response.GetDocument();
                doc.GetElementByTestId("postcode").TextContent.Should().Be(postcode);

                var resultOptions = doc.GetElementById("AddressId").QuerySelectorAll("option")
                    .Select(o => (Id: o.GetAttribute("value"), Text: o.TextContent))
                    .Skip(1)  // first N addresses found option
                    .ToArray();

                resultOptions.Should().BeEquivalentTo(new[]
                {
                    ("result1", "Test Address 1"),
                    ("result2", "Test Address 2"),
                    ("result3", "Test Address 3"),
                });
            }
        }

        [Fact]
        public async Task PostcodeSearch_ValidRequest_NormalizesPostcode()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var inputPostcode = "ab12de";

            var town = "Town";
            var country = "England";
            var normalizedPostcode = "AB1 2DE";

            var postcodeLatitude = 42D;
            var postcodeLongitude = 43D;

            await TestData.CreatePostcodeInfo(normalizedPostcode, postcodeLatitude, postcodeLongitude, country == "England");

            ConfigureAddressSearchServiceResults(normalizedPostcode, town, resultCount: 3);

            var journeyInstance = CreateJourneyInstance(provider.ProviderId);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/venues/add/postcode-search?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}&postcode={inputPostcode}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (new AssertionScope())
            {
                var doc = await response.GetDocument();
                doc.GetElementByTestId("postcode").TextContent.Should().Be(normalizedPostcode);
            }
        }

        [Fact]
        public async Task SelectPostcode_NoAddressSelected_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var town = "Town";
            var country = "England";
            var postcode = "AB1 2DE";

            var postcodeLatitude = 42D;
            var postcodeLongitude = 43D;

            await TestData.CreatePostcodeInfo(postcode, postcodeLatitude, postcodeLongitude, country == "England");

            var postcodeSearchResults = ConfigureAddressSearchServiceResults(postcode, town, resultCount: 3);

            var journeyInstance = CreateJourneyInstance(
                provider.ProviderId,
                new AddVenueJourneyModel()
                {
                    PostcodeSearchResults = postcodeSearchResults,
                    PostcodeSearchQuery = postcode
                });

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/venues/add/select-postcode?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}&postcode={postcode}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("AddressId", "Select an address");
        }

        [Fact]
        public async Task SelectPostcode_InvalidAddressSelected_ReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var addressId = "42";
            var town = "Town";
            var country = "England";
            var postcode = "AB1 2DE";

            var postcodeLatitude = 42D;
            var postcodeLongitude = 43D;

            await TestData.CreatePostcodeInfo(postcode, postcodeLatitude, postcodeLongitude, country == "England");

            var postcodeSearchResults = ConfigureAddressSearchServiceResults(postcode, town, resultCount: 3);

            var journeyInstance = CreateJourneyInstance(
                provider.ProviderId,
                new AddVenueJourneyModel()
                {
                    PostcodeSearchResults = postcodeSearchResults,
                    PostcodeSearchQuery = postcode
                });

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/venues/add/select-postcode?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}&postcode={postcode}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("AddressId", addressId)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("England", false)]
        [InlineData("Scotland", true)]
        public async Task SelectPostcode_ValidRequest_UpdatesJourneyStateAndRedirects(
            string onspdRecordCountry,
            bool expectedNewAddressIsOutsideOfEnglandValue)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var addressId = "42";
            var addressLine1 = "Test Venue line 1";
            var addressLine2 = "Test Venue line 2";
            var town = "Town";
            var county = "County";
            var postcode = "AB1 2DE";

            var postcodeLatitude = 42D;
            var postcodeLongitude = 43D;

            await TestData.CreatePostcodeInfo(postcode, postcodeLatitude, postcodeLongitude, onspdRecordCountry == "England");

            var postcodeSearchResults = new[]
            {
                new PostcodeSearchResult()
                {
                    Id = "result1",
                    StreetAddress = $"{addressLine1} {addressLine2}",
                    Place = town
                }
            };

            var journeyInstance = CreateJourneyInstance(
                provider.ProviderId,
                new AddVenueJourneyModel()
                {
                    PostcodeSearchResults = postcodeSearchResults,
                    PostcodeSearchQuery = postcode
                });

            AddressSearchService
                .Setup(s => s.GetById(addressId))
                .ReturnsAsync(new AddressDetail()
                {
                    CountryName = onspdRecordCountry,
                    County = county,
                    Line1 = addressLine1,
                    Line2 = addressLine2,
                    Postcode = postcode,
                    PostTown = town
                });

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/venues/add/select-postcode?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}&postcode={postcode}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("AddressId", addressId)
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

        private PostcodeSearchResult[] ConfigureAddressSearchServiceResults(string postcode, string town, int resultCount)
        {
            var results = Enumerable.Range(1, resultCount)
                .Select(index => new PostcodeSearchResult()
                {
                    Id = $"result{index}",
                    StreetAddress = $"Test Address {index}",
                    Place = town
                })
                .ToArray();

            AddressSearchService
                .Setup(s => s.SearchByPostcode(postcode))
                .ReturnsAsync(results);

            return results;
        }

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
