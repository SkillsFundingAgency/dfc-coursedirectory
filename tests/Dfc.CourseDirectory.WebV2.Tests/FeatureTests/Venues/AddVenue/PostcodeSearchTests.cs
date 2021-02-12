using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
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
            var providerId = await TestData.CreateProvider();

            var postcode = "xxx";

            var journeyInstance = CreateJourneyInstance(providerId);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/venues/add/postcode-search?providerId={providerId}&ffiid={journeyInstance.InstanceId.UniqueKey}&postcode={postcode}");

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
            var providerId = await TestData.CreateProvider();

            var town = "Town";
            var postcode = "AB1 2DE";

            OnspdSearchClient
                .Setup(c => c.Search(It.Is<OnspdSearchQuery>(q => q.Postcode == postcode)))
                .ReturnsAsync(new SearchResult<Onspd>()
                {
                    Items = Array.Empty<SearchResultItem<Onspd>>()
                });

            ConfigureAddressSearchServiceResults(postcode, town, resultCount: 3);

            var journeyInstance = CreateJourneyInstance(providerId);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/venues/add/postcode-search?providerId={providerId}&ffiid={journeyInstance.InstanceId.UniqueKey}&postcode={postcode}");

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
            var providerId = await TestData.CreateProvider();

            var town = "Town";
            var country = "England";
            var postcode = "AB1 2DE";

            var postcodeLatitude = 42M;
            var postcodeLongitude = 43M;

            ConfigureOnspdLookupForPostcode(postcode, country, postcodeLatitude, postcodeLongitude);

            ConfigureAddressSearchServiceResults(postcode, town, resultCount: 0);

            var journeyInstance = CreateJourneyInstance(providerId);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/venues/add/postcode-search?providerId={providerId}&ffiid={journeyInstance.InstanceId.UniqueKey}&postcode={postcode}");

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
            var providerId = await TestData.CreateProvider();

            var town = "Town";
            var country = "England";
            var postcode = "AB1 2DE";

            var postcodeLatitude = 42M;
            var postcodeLongitude = 43M;

            ConfigureOnspdLookupForPostcode(postcode, country, postcodeLatitude, postcodeLongitude);

            ConfigureAddressSearchServiceResults(postcode, town, resultCount: 3);

            var journeyInstance = CreateJourneyInstance(providerId);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/venues/add/postcode-search?providerId={providerId}&ffiid={journeyInstance.InstanceId.UniqueKey}&postcode={postcode}");

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
        public async Task SelectPostcode_NoAddressSelected_ReturnsError()
        {
            // Arrange
            var providerId = await TestData.CreateProvider();

            var town = "Town";
            var country = "England";
            var postcode = "AB1 2DE";

            var postcodeLatitude = 42M;
            var postcodeLongitude = 43M;

            ConfigureOnspdLookupForPostcode(postcode, country, postcodeLatitude, postcodeLongitude);

            ConfigureAddressSearchServiceResults(postcode, town, resultCount: 3);

            var journeyInstance = CreateJourneyInstance(providerId);

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/venues/add/select-postcode?providerId={providerId}&ffiid={journeyInstance.InstanceId.UniqueKey}&postcode={postcode}")
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
        public async Task SelectPostcode_InvalidAddressSelected_ReturnsError()
        {
            // Arrange
            var providerId = await TestData.CreateProvider();

            var addressId = "42";
            var town = "Town";
            var country = "England";
            var postcode = "AB1 2DE";

            var postcodeLatitude = 42M;
            var postcodeLongitude = 43M;

            ConfigureOnspdLookupForPostcode(postcode, country, postcodeLatitude, postcodeLongitude);

            ConfigureAddressSearchServiceResults(postcode, town, resultCount: 3);

            var journeyInstance = CreateJourneyInstance(providerId);

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/venues/add/select-postcode?providerId={providerId}&ffiid={journeyInstance.InstanceId.UniqueKey}&postcode={postcode}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("AddressId", addressId)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("AddressId", "Select an address");
        }

        [Theory]
        [InlineData("England", false)]
        [InlineData("Scotland", true)]
        public async Task SelectPostcode_ValidRequest_UpdatesJourneyStateAndRedirects(
            string onspdRecordCountry,
            bool expectedNewAddressIsOutsideOfEnglandValue)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();

            var addressId = "42";
            var addressLine1 = "Test Venue line 1";
            var addressLine2 = "Test Venue line 2";
            var town = "Town";
            var county = "County";
            var postcode = "AB1 2DE";

            var postcodeLatitude = 42M;
            var postcodeLongitude = 43M;

            ConfigureOnspdLookupForPostcode(postcode, onspdRecordCountry, postcodeLatitude, postcodeLongitude);

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

            var journeyInstance = CreateJourneyInstance(providerId);

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/venues/add/select-postcode?providerId={providerId}&ffiid={journeyInstance.InstanceId.UniqueKey}&postcode={postcode}")
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
                .Be($"/venues/add/details?providerId={providerId}&ffiid={journeyInstance.InstanceId.UniqueKey}");

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

        private void ConfigureOnspdLookupForPostcode(string postcode, string country, decimal latitude, decimal longitude)
        {
            OnspdSearchClient
                .Setup(c => c.Search(It.Is<OnspdSearchQuery>(q => q.Postcode == postcode)))
                .ReturnsAsync(new SearchResult<Onspd>()
                {
                    Items = new[]
                    {
                        new SearchResultItem<Onspd>()
                        {
                            Record = new Onspd()
                            {
                                pcds = postcode,
                                Country = country,
                                lat = latitude,
                                @long = longitude
                            }
                        }
                    }
                });
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
