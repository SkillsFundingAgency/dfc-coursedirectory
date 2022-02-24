using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
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

            // Single wildcard
            yield return new object[] { "*", "*" };

            // Input is trimmed
            yield return new object[] { " foo  ", "foo* || foo~" };

            // Add wildcard and fuzzy modifier to end of each word
            yield return new object[] { "foo bar", "foo* || foo~ || bar* || bar~" };

            // Escaped characters don't get added
            yield return new object[] { "foo *", "foo* || foo~" };

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

            // Includes 'T Level'
            yield return new object[] { "T Level", "\"T Level\"" };
            yield return new object[] { "T-Level", "\"T-Level\"" };
            yield return new object[] { "T Level computer science", "\"T Level\" AND (computer* || computer~ || science* || science~)" };

            // Combinations...
            yield return new object[] { "foo 'bar baz' \"qux qu|ux\"", "(bar && baz) || (\"qux qu\\|ux\") || foo* || foo~" };
        }

        private FindACourseOfferingSearchQuery CapturedQuery { get; set; }

        [Fact]
        public async Task SortByDistanceButNoPostcodeOrLatLng_ReturnsError()
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

        [Fact]
        public async Task LatSpecifiedWithoutLng_ReturnsError()
        {
            // Arrange
            var request = CreateRequest(new
            {
                latitude = 2D
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertHaveError(
                response,
                title: "InvalidLatLng",
                detail: "Latitude & longitude must both be specified.");
        }

        [Fact]
        public async Task LngSpecifiedWithoutLat_ReturnsError()
        {
            // Arrange
            var request = CreateRequest(new
            {
                longitude = 2D
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertHaveError(
                response,
                title: "InvalidLatLng",
                detail: "Latitude & longitude must both be specified.");
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
        public async Task HideFlexiCoursesSpecified_AddsStartDateFilterToQuery()
        {
            // Arrange
            var request = CreateRequest(new
            {
                hideFlexiCourses = true
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().Contain(
                $"{nameof(FindACourseOffering.StartDate)} ne null");
        }


        [Fact]
        public async Task HideFlexiCoursesNotSpecified_WithStartDateFromSpecified_AddsStartDateFilterWithOrToQuery()
        {
            // Arrange
            var startDateFrom = new DateTime(2020, 4, 1);

            var request = CreateRequest(new
            {
                startDateFrom = startDateFrom,
                hideFlexiCourses = false
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().Contain(
                $"({nameof(FindACourseOffering.StartDate)} ge {startDateFrom:o}) or {nameof(FindACourseOffering.StartDate)} eq null");
        }

        [Fact]
        public async Task HideFlexiCoursesNotSpecified_WithStartDateToSpecified_AddsStartDateFilterWithOrToQuery()
        {
            // Arrange
            var startDateTo = new DateTime(2020, 4, 1);

            var request = CreateRequest(new
            {
                startDateTo = startDateTo,
                hideFlexiCourses = false
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().Contain(
                $"({nameof(FindACourseOffering.StartDate)} le {startDateTo:o}) or {nameof(FindACourseOffering.StartDate)} eq null");
        }

        [Fact]
        public async Task HideOutOfDateCoursesSpecified_AddsStartDateFilterToQuery()
        {
            // Arrange
            var request = CreateRequest(new
            {
                hideOutOfDateCourses = true
            });

            int DefaultStartFromThreshold = 60;
            var outOfDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day)
                .AddDays(DefaultStartFromThreshold * -1);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().Contain(
                $"{nameof(FindACourseOffering.StartDate)} ge {outOfDate:o}");
        }

        [Fact]
        public async Task HideOutOfDateCoursesSpecified_And_Not_HideFlexiCoursesSpecified_AddsStartDateFilterToQuery()
        {
            // Arrange
            var request = CreateRequest(new
            {
                hideOutOfDateCourses = true,
                hideFlexiCourses = false
            });

            int DefaultStartFromThreshold = 60;
            var outOfDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)
                .AddDays(DefaultStartFromThreshold * -1);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().Contain(
                $"({nameof(FindACourseOffering.StartDate)} ge {outOfDate:o}) or {nameof(FindACourseOffering.StartDate)} eq null");
        }

        [Fact]
        public async Task HideOutOfDateCoursesSpecified_And_HideFlexiCoursesSpecified_AddsStartDateFilterToQuery()
        {
            // Arrange
            var request = CreateRequest(new
            {
                hideOutOfDateCourses = true,
                hideFlexiCourses = true
            });

            int DefaultStartFromThreshold = 60;
            var outOfDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day)
                .AddDays(DefaultStartFromThreshold * -1);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().Contain(
                $"{nameof(FindACourseOffering.StartDate)} ne null and { nameof(FindACourseOffering.StartDate)} ge {outOfDate:o}");
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
                $"{nameof(FindACourseOffering.StartDate)} ge {startDateFrom:o}");

            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().NotContain(
                $" or {nameof(FindACourseOffering.StartDate)} eq null");
        }


        [Fact]
        public async Task StartDateFromSpecified_And_ShowFlexiCoursesSpecified_AddsFilterToQuery()
        {
            // Arrange
            var startDateFrom = new DateTime(2020, 4, 1);

            var request = CreateRequest(new
            {
                startDateFrom = startDateFrom,
                hideFlexiCourses = false
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().Contain(
                $" or {nameof(FindACourseOffering.StartDate)} eq null");
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
                $"{nameof(FindACourseOffering.StartDate)} le {startDateTo:o}");
        }

        [Fact]
        public async Task StartDateToSpecified_And_ShowFlexiCoursesSpecified_AddsFilterToQuery()
        {
            // Arrange
            var startDateTo = new DateTime(2021, 12, 31);

            var request = CreateRequest(new
            {
                startDateTo = startDateTo,
                hideFlexiCourses = false
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().Contain(
                $" or {nameof(FindACourseOffering.StartDate)} eq null");
        }

        [Fact]
        public async Task StartDateToAndFromSpecified_AddsFilterToQuery()
        {
            // Arrange
            var startDateTo = new DateTime(2021, 12, 31);
            var startDateFrom = new DateTime(2020, 4, 1);

            var request = CreateRequest(new
            {
                startDateTo = startDateTo,
                startDateFrom = startDateFrom
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().Contain(
                $"{nameof(FindACourseOffering.StartDate)} ge {startDateFrom:o} and StartDate le {startDateTo:o}");
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
                "(AttendancePattern eq 2 or AttendancePattern eq 3 or DeliveryMode ne 1) and Live eq true");
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
        public async Task DistanceSpecifiedWithPostcode_AddsFilterToQuery()
        {
            // Arrange
            var postcode = "AB1 2DE";
            var lat = 1D;
            var lng = 2D;
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
        public async Task DistanceSpecifiedWithLatLng_AddsFilterToQuery()
        {
            // Arrange
            var lat = 1m;
            var lng = 2m;

            var distance = 10;
            var distanceInKm = GeoHelper.MilesToKilometers(10);

            var request = CreateRequest(new
            {
                sortBy = "Distance",
                latitude = lat,
                longitude = lng,
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
        public async Task LatLngAndPostcodeSpecifiedUsesLatLngOverPostcode_AddsFilterToQuery()
        {
            // Arrange
            var postcode = "AB1 2DE";
            var postcodeLat = 1D;
            var postcodeLng = 2D;
            ConfigureOnspdSearchResultsForPostcode(postcode, coords: (postcodeLat, postcodeLng));

            var lat = 5d;
            var lng = 6d;

            var distance = 10;
            var distanceInKm = GeoHelper.MilesToKilometers(10);

            var request = CreateRequest(new
            {
                sortBy = "Distance",
                postcode = postcode,
                latitude = lat,
                longitude = lng,
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
                "(StudyMode eq 1 or StudyMode eq 3 or DeliveryMode ne 1)");
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
            CapturedQuery.GenerateSearchQuery().Options.Filter.Should().Contain("Live eq true");
        }

        [Theory]
        [InlineData(1, "search.score() desc")]  // Relevance
        [InlineData(2, "StartDate desc")]  // StartDateDescending
        [InlineData(3, "StartDate asc")]  // StartDateAscending
        [InlineData(4, "geo.distance(Position, geography'POINT(2 1)')")]  // Distance
        public async Task OrderByIsCorrectlyDeduced(int sortBy, string expectedOrderByClause)
        {
            // Arrange
            var postcode = "AB1 2DE";
            var lat = 1D;
            var lng = 2D;
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

        [Fact]
        public async Task PostcodeWithoutSpaces_IsNormalizedToIncludeSpace()
        {
            // Arrange
            var postcode = "AB12DE";
            var normalizedPostcode = "AB1 2DE";
            var lat = 1D;
            var lng = 2D;
            ConfigureOnspdSearchResultsForPostcode(normalizedPostcode, coords: (lat, lng));

            var request = CreateRequest(new
            {
                postcode,
                sortBy = "Distance"
            });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            SqlQueryDispatcher.Verify(c => c.ExecuteQuery(It.Is<GetPostcodeInfo>(q => q.Postcode == normalizedPostcode)));
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
            (double lat, double lng)? coords)
        {
            if (coords.HasValue)
            {
                SqlQueryDispatcher.Setup(mock => mock.ExecuteQuery(It.Is<GetPostcodeInfo>(q => q.Postcode == postcode)))
                    .ReturnsAsync(new Core.DataStore.Sql.Models.PostcodeInfo()
                    {
                        InEngland = true,
                        Latitude = coords.Value.lat,
                        Longitude = coords.Value.lng,
                        Postcode = postcode
                    });
            }
        }
    }
}
