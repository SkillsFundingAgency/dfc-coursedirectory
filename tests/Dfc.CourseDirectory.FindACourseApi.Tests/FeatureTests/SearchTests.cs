using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Dfc.CourseDirectory.FindACourseApi.Tests.FeatureTests
{
    public class SearchTests : TestBase
    {
        public SearchTests(FindACourseApiApplicationFactory factory)
            : base(factory)
        {
            FindACourseOfferingSearchClient
                .Setup(c => c.Search(It.IsAny<FindACourseOfferingSearchQuery>()))
                .Callback<FindACourseOfferingSearchQuery>(q => CapturedQuery = q)
                .ReturnsAsync(new SearchResult<FindACourseOffering>()
                {
                    Facets = new Dictionary<string, IReadOnlyDictionary<object, long?>>()
                    {
                        { "NotionalNVQLevelv2", new Dictionary<object, long?>() },
                        { "VenueStudyMode", new Dictionary<object, long?>() },
                        { "VenueAttendancePattern", new Dictionary<object, long?>() },
                        { "DeliveryMode", new Dictionary<object, long?>() },
                        { "ProviderName", new Dictionary<object, long?>() },
                        { "Region", new Dictionary<object, long?>() },
                    },
                    Items = Array.Empty<SearchResultItem<FindACourseOffering>>(),
                    TotalCount = 0
                });
        }

        public static IEnumerable<object[]> TranslateCourseSearchSubjectTextData()
        {
            // Empty input should be mapped to wildcard
            yield return new object[] { null, "*" };
            yield return new object[] { "", "*" };
            yield return new object[] { " ", "*" };
            yield return new object[] { "   ", "*" };

            // Input is trimmed
            yield return new object[] { " foo  ", "foo* || foo~" };

            // Add wildcard and fuzzy modifier to end of each word
            yield return new object[] { "foo bar", "foo* || foo~ || bar* || bar~" };

            // Terms in single quotes should not be prefix searches
            yield return new object[] { "'foo'", "(foo)" };

            // Single quote grouping
            yield return new object[] { "'foo' 'bar baz'", "(foo) || (bar && baz)" };

            // Double quotes
            yield return new object[] { "\"foo\"", "(\"foo\")" };
            yield return new object[] { "\"foo bar\"", "(\"foo bar\")" };

            // Escapes special characters
            yield return new object[] { "'foo+bar'", "(foo\\+bar)" };
            yield return new object[] { "'foo|bar'", "(foo\\|bar)" };
            yield return new object[] { "'foo-bar'", "(foo\\-bar)" };
            yield return new object[] { "'foo*bar'", "(foo\\*bar)" };
            yield return new object[] { "'foo(bar'", "(foo\\(bar)" };
            yield return new object[] { "'foo)bar'", "(foo\\)bar)" };

            // Combinations...
            yield return new object[] { "foo 'bar baz' \"qux qu|ux\"", "(bar && baz) || (\"qux qu\\|ux\") || foo* || foo~" };
        }

        private FindACourseOfferingSearchQuery CapturedQuery { get; set; }

        [Fact]
        public async Task SortByDistanceButEmptyPostcode_ReturnsError()
        {
            // Arrange
            var request = CreateRequest(new
            {
                sortBy = "Distance"
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertHaveError(
                response,
                title: "PostcodeRequired",
                detail: "Postcode is required to sort by Distance.");
        }

        [Theory]
        [InlineData("x")]
        // Following data are commented out because it would be a breaking API change
        //[InlineData("")]
        //[InlineData(" ")]
        public async Task InvalidPostcode_ReturnsError(string postcode)
        {
            // Arrange
            var request = CreateRequest(new
            {
                postcode
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertHaveError(
                response,
                title: "InvalidPostcode",
                detail: "Postcode is not valid.");
        }

        [Fact]
        public async Task PostcodeLookupFails_ReturnsError()
        {
            // Arrange
            var postcode = "AB1 2DE";
            ConfigureOnspdSearchResultsForPostcode(postcode, coords: null);

            var request = CreateRequest(new
            {
                postcode,
                sortBy = "Distance"
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertHaveError(
                response,
                title: "PostcodeNotFound",
                detail: "Specified postcode cannot be found.");
        }

        [Fact]
        public async Task StartDateFromSpecified_AddsFilterToQuery()
        {
            // Arrange
            var startDateFrom = new DateTime(2020, 4, 1);

            var request = CreateRequest(new
            {
                startDateFrom = startDateFrom
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().Contain(
                $"StartDate ge {startDateFrom:o}");
        }

        [Fact]
        public async Task StartDateToSpecified_AddsFilterToQuery()
        {
            // Arrange
            var startDateTo = new DateTime(2021, 12, 31);

            var request = CreateRequest(new
            {
                startDateTo = startDateTo
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().Contain(
                $"StartDate le {startDateTo:o}");
        }

        [Fact]
        public async Task AttendancePatternsSpecified_AddsFilterToQuery()
        {
            // Arrange
            var request = CreateRequest(new
            {
                attendancePatterns = new[]
                {
                    2,  // Evening
                    3  // Weekend
                }
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().Contain(
                "(AttendancePattern eq 2 or AttendancePattern eq 3) and Live eq true");
        }

        [Fact]
        public async Task QualificationLevelsSpecified_AddsFilterToQuery()
        {
            // Arrange
            var request = CreateRequest(new
            {
                qualificationLevels = new[] { "3", "X" }
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().Contain(
                "search.in(NotionalNVQLevelv2, '3|X', '|')");
        }

        [Fact]
        public async Task DistanceSpecified_AddsFilterToQuery()
        {
            // Arrange
            var postcode = "AB1 2DE";
            var lat = 1m;
            var lng = 2m;
            ConfigureOnspdSearchResultsForPostcode(postcode, coords: (lat, lng));

            var distance = 10;
            var distanceInKm = GeoHelper.MilesToKilometers(10);

            var request = CreateRequest(new
            {
                sortBy = "Distance",
                postcode = postcode,
                distance = distance
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().Contain(
                $"geo.distance(Position, geography'POINT({lng} {lat})') le {distanceInKm}");
        }

        [Fact]
        public async Task TownSpecified_AddsFilterToQuery()
        {
            // Arrange
            var request = CreateRequest(new
            {
                town = "Coventry"
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().Contain(
                "search.ismatch('Coventry', 'VenueTown')");
        }

        [Fact]
        public async Task StudyModesSpecified_AddsFilterToQuery()
        {
            // Arrange
            var request = CreateRequest(new
            {
                studyModes = new[]
                {
                    1,  // FullTime
                    3  // Flexible
                }
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().Contain(
                "(StudyMode eq 1 or StudyMode eq 3)");
        }

        [Fact]
        public async Task DeliveryModesSpecified_AddsFilterToQuery()
        {
            // Arrange
            var request = CreateRequest(new
            {
                deliveryModes = new[]
                {
                    1,  // ClassroomBased
                    3  // WorkBased
                }
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().Contain(
                "(DeliveryMode eq 1 or DeliveryMode eq 3)");
        }

        [Fact]
        public async Task ProviderNameSpecified_AddsFilterToQuery()
        {
            // Arrange
            var request = CreateRequest(new
            {
                providerName = "Test Provider"
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().Contain(
                "search.ismatchscoring('Test Provider', 'ProviderDisplayName', 'simple', 'any')");
        }

        [Fact]
        public async Task LiveFilterIsAdded()
        {
            // Arrange
            var request = CreateRequest(new { });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().Be("Live eq true");
        }

        [Theory]
        [InlineData(1, "search.score() desc")]  // Relevance
        [InlineData(2, "StartDate desc")]  // StartDateDescending
        [InlineData(3, "StartDate asc")]  // StartDateDescending
        [InlineData(4, "geo.distance(VenueLocation, geography'POINT(2 1)')")]  // Distance
        public async Task OrderByIsCorrectlyDeduced(int sortBy, string expectedOrderByClause)
        {
            // Arrange
            var postcode = "AB1 2DE";
            var lat = 1m;
            var lng = 2m;
            ConfigureOnspdSearchResultsForPostcode(postcode, coords: (lat, lng));

            var request = CreateRequest(new
            {
                sortBy = sortBy,
                postcode = postcode
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.OrderBy.Single().Should().Be(expectedOrderByClause);
        }

        [Theory]
        [InlineData(0, "limit parameter is invalid.")]
        [InlineData(-1, "limit parameter is invalid.")]
        [InlineData(51, "limit parameter cannot be greater than 50.")]
        public async Task InvalidLimitSpecified_ReturnsError(int limit, string expectedErrorDetail)
        {
            // Arrange
            var request = CreateRequest(new
            {
                limit = limit
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertHaveError(
                response,
                title: "InvalidPagingParameters",
                detail: expectedErrorDetail);
        }

        [Fact]
        public async Task ValidLimitSpecified_IsPassedToQuery()
        {
            // Arrange
            var limit = 15;

            var request = CreateRequest(new
            {
                limit = limit
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Size.Should().Be(limit);
        }

        [Fact]
        public async Task NoLimitSpecified_UsesDefault()
        {
            // Arrange
            var request = CreateRequest(new { });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Size.Should().Be(20);
        }

        [Theory]
        [InlineData(-1)]
        public async Task InvalidStartSpecified_ReturnsError(int start)
        {
            // Arrange
            var request = CreateRequest(new
            {
                start = start
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertHaveError(
                response,
                title: "InvalidPagingParameters",
                detail: "Start parameter is invalid.");
        }

        [Fact]
        public async Task ValidStartSpecified_IsPassedToQuery()
        {
            // Arrange
            var request = CreateRequest(new
            {
                start = 5
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Skip.Should().Be(5);
        }

        [Fact]
        public async Task NoStartSpecified_UsesDefault()
        {
            // Arrange
            var request = CreateRequest(new { });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Skip.Should().Be(0);
        }

        [Theory]
        [MemberData(nameof(TranslateCourseSearchSubjectTextData))]
        public async Task SubjectKeyword_IsAddedToQuery(string subjectKeyword, string expectedSearchText)
        {
            // Arrange
            var request = CreateRequest(new
            {
                subjectKeyword = subjectKeyword
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().SearchText.Should().Be(expectedSearchText);
        }

        private static async Task AssertHaveError(HttpResponseMessage response, string title, string detail)
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
            var responseJson = JObject.Parse(await response.Content.ReadAsStringAsync());

            using (new AssertionScope())
            {
                responseJson["title"]?.ToString().Should().Be(title);
                responseJson["detail"]?.ToString().Should().Be(detail);
            }
        }

        private static HttpRequestMessage CreateRequest(object requestBody)
        {
            var content = new StringContent(JsonConvert.SerializeObject(requestBody));
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            return new HttpRequestMessage(HttpMethod.Post, "search")
            {
                Content = content
            };
        }

        private void ConfigureOnspdSearchResultsForPostcode(
            string postcode,
            (decimal lat, decimal lng)? coords)
        {
            OnspdSearchClient
                .Setup(c => c.Search(It.Is<OnspdSearchQuery>(q => q.Postcode == postcode)))
                .ReturnsAsync(new SearchResult<Onspd>()
                {
                    Items = coords != null ?
                        new[]
                        {
                            new SearchResultItem<Onspd>()
                            {
                                Record = new Onspd()
                                {
                                    pcd2 = postcode,
                                    lat = coords.Value.lat,
                                    @long = coords.Value.lng
                                }
                            }
                        } :
                        Array.Empty<SearchResultItem<Onspd>>(),
                    TotalCount = coords != null ? 1 : 0
                });
        }
    }
}
