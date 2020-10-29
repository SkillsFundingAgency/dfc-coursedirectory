using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dfc.CourseDirectory.FindACourseApi.Interfaces;
using Dfc.CourseDirectory.FindACourseApi.Models;
using Dfc.GeoCoordinate;
using Dfc.ProviderPortal.Packages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.FindACourseApi.Helpers
{
    public class SearchServiceWrapper
    {
        private class LINQComboClass
        {
            public Course Course { get; set; }
            public CourseRun Run { get; set; }
            public string Region { get; set; }
            public AzureSearchVenueModel Venue { get; set; }
        }

        private static readonly HashSet<char> _luceneSyntaxEscapeChars = new HashSet<char>("+-&|!(){}[]^\"~*?:\\/");

        private static readonly Regex _postcode = new Regex("^([A-PR-UWYZ0-9][A-HK-Y0-9][AEHMNPRTVXY0-9]?[ABEHMNPRVWXY0-9]? {1,2}[0-9][ABD-HJLN-UW-Z]{2}|GIR 0AA)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly ILogger _log;
        private readonly ISearchServiceSettings _settings;
        private readonly SearchServiceClient _queryService;
        private readonly ISearchIndexClient _queryIndex;
        private readonly ISearchIndexClient _onspdIndex;
        private readonly HttpClient _httpClient;

        public SearchServiceWrapper(ISearchServiceSettings settings, ILoggerFactory loggerFactory)
        {
            Throw.IfNull(loggerFactory, nameof(loggerFactory));
            Throw.IfNull(settings, nameof(settings));

            _log = loggerFactory.CreateLogger<SearchServiceWrapper>();
            _settings = settings;

            _queryService = new SearchServiceClient(settings.SearchService, new SearchCredentials(settings.QueryKey));
            _queryIndex = _queryService?.Indexes?.GetClient(settings.Index);
            _onspdIndex = _queryService?.Indexes?.GetClient(settings.onspdIndex);

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("api-key", settings.QueryKey);
            _httpClient.DefaultRequestHeaders.Add("api-version", settings.ApiVersion);
            _httpClient.DefaultRequestHeaders.Add("indexes", settings.Index);
        }

        public async Task<FACSearchResult> SearchCourses(SearchCriteriaStructure criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));

            _log.LogInformation("FAC search criteria.", criteria);

            var sortBy = criteria.SortBy ?? CourseSearchSortBy.Relevance;

            // Can only sort by distance if a postcode is specified
            if (sortBy == CourseSearchSortBy.Distance && string.IsNullOrWhiteSpace(criteria.Postcode))
            {
                throw new ProblemDetailsException(new ProblemDetails()
                {
                    Detail = "Postcode is required to sort by Distance.",
                    Status = 400,
                    Title = "PostcodeRequired"
                });
            }

            // Validate postcode
            if (!string.IsNullOrWhiteSpace(criteria.Postcode))
            {
                if (!_postcode.IsMatch(criteria.Postcode))
                {
                    throw new ProblemDetailsException(new ProblemDetails()
                    {
                        Detail = "Postcode is not valid.",
                        Status = 400,
                        Title = "InvalidPostcode"
                    });
                }
            }

            var geoFilterRequired = criteria.Distance.GetValueOrDefault(0) > 0 && !string.IsNullOrWhiteSpace(criteria.Postcode);

            // lat/lng required if Distance filter is specified *or* sorting by Distance
            var getBaseCoords = geoFilterRequired || sortBy == CourseSearchSortBy.Distance;
            float? latitude = null;
            float? longitude = null;
            if (getBaseCoords)
            {
                _log.LogInformation($"FAC getting lat/long for location {criteria.Postcode}");

                var coords = await TryGetCoordinatesForPostcode(criteria.Postcode);

                if (!coords.HasValue)
                {
                    throw new ProblemDetailsException(new ProblemDetails()
                    {
                        Detail = "Specified postcode cannot be found.",
                        Status = 400,
                        Title = "PostcodeNotFound"
                    });
                }

                latitude = coords.Value.lat;
                longitude = coords.Value.lng;
            }

            var filterClauses = new List<string>()
            {
                "Status eq 1"  // only search live courses
            };

            if (criteria.StartDateFrom.HasValue)
            {
                filterClauses.Add($"StartDate ge {criteria.StartDateFrom.Value:o}");
            }

            if (criteria.StartDateTo.HasValue)
            {
                filterClauses.Add($"StartDate le {criteria.StartDateTo.Value:o}");
            }

            if (criteria.AttendancePatterns?.Any() ?? false)
            {
                filterClauses.Add($"search.in(VenueAttendancePattern, '{string.Join("|", criteria.AttendancePatterns)}', '|')");
            }

            if (criteria.QualificationLevels?.Any() ?? false)
            {
                filterClauses.Add($"search.in(NotionalNVQLevelv2, '{string.Join("|", criteria.QualificationLevels.Select(EscapeFilterValue))}', '|')");
            }

            if (geoFilterRequired)
            {
                var distanceInKm = GeoHelper.MilesToKilometers(criteria.Distance.Value);
                filterClauses.Add(
                    $"(geo.distance(VenueLocation, geography'POINT({longitude.Value} {latitude.Value})') le {distanceInKm}" +
                    " or National eq true" +
                    " or DeliveryMode eq '2')");
            }

            if (!string.IsNullOrWhiteSpace(criteria.Town))
            {
                var townEscaped = EscapeFilterValue(criteria.Town);
                filterClauses.Add($"search.ismatch('{townEscaped}', 'VenueTown')");
            }

            if (criteria.StudyModes?.Any() ?? false)
            {
                filterClauses.Add($"search.in(VenueStudyMode, '{string.Join("|", criteria.StudyModes)}', '|')");
            }

            if (criteria.DeliveryModes?.Any() ?? false)
            {
                filterClauses.Add($"search.in(DeliveryMode, '{string.Join("|", criteria.DeliveryModes)}', '|')");
            }

            if (!string.IsNullOrWhiteSpace(criteria.ProviderName))
            {
                var providerNameEscaped = EscapeFilterValue(criteria.ProviderName);
                filterClauses.Add($"search.ismatchscoring('{providerNameEscaped}', 'ProviderName', 'simple', 'any')");
            }

            var filter = string.Join(" and ", filterClauses);

            var orderBy = sortBy == CourseSearchSortBy.StartDateDescending ?
                "StartDate desc" : sortBy == CourseSearchSortBy.StartDateAscending ?
                "StartDate asc" : sortBy == CourseSearchSortBy.Distance ?
                $"geo.distance(VenueLocation, geography'POINT({longitude.Value} {latitude.Value})')" :
                "search.score() desc";

            var (limit, start) = ResolvePagingParams(criteria.Limit, criteria.Start);

            var scoringProfile = string.IsNullOrWhiteSpace(_settings.RegionBoostScoringProfile) ? "region-boost" : _settings.RegionBoostScoringProfile;

            var searchText = TranslateCourseSearchSubjectText(criteria.SubjectKeyword);

            var results = await _queryIndex.Documents.SearchAsync<AzureSearchCourse>(
                searchText,
                new SearchParameters()
                {
                    Facets = new[]
                    {
                        "NotionalNVQLevelv2,count:100",
                        "VenueStudyMode",
                        "VenueAttendancePattern",
                        "DeliveryMode",
                        "ProviderName,count:100",
                        "Region,count:100"
                    },
                    Filter = filter,
                    IncludeTotalResultCount = true,
                    SearchFields = new[]
                    {
                        "QualificationCourseTitle",
                        "CourseName",
                    },
                    ScoringProfile = scoringProfile,
                    SearchMode = SearchMode.All,
                    QueryType = QueryType.Full,
                    Top = limit,
                    Skip = start,
                    OrderBy = new[] { orderBy }
                });

            return new FACSearchResult()
            {
                Limit = limit,
                Start = start,
                Total = (int)results.Count.Value,
                Facets = results.Facets,
                Items = results.Results.Select(r => new FACSearchResultItem()
                {
                    Course = r.Document,
                    Distance = getBaseCoords && r.Document.VenueLocation != null ?
                        Math.Round(
                            GeoHelper.KilometersToMiles(
                                GeoHelper.GetDistanceTo(
                                    (latitude.Value, longitude.Value),
                                    (r.Document.VenueLocation.Latitude, r.Document.VenueLocation.Longitude))),
                            2) :
                        (double?)null,
                    Score = r.Score
                })
            };

            string EscapeFilterValue(string v) => v.Replace("'", "''");
        }

        public static string TranslateCourseSearchSubjectText(string subjectText)
        {
            if (string.IsNullOrWhiteSpace(subjectText))
            {
                return "*";
            }

            var terms = new List<string>();

            // Find portions wrapped in quotes
            var remaining = subjectText.Trim();
            var groupsRegex = new Regex("('|\")(.*?)(\\1)");
            Match m;
            while ((m = groupsRegex.Match(remaining)).Success)
            {
                var value = EscapeSearchText(m.Groups[2].Value);

                if (m.Groups[1].Value == "'")
                {
                    terms.Add($"({CombineWords(value, " && ")})");
                }
                else   // double quotes
                {
                    terms.Add($"(\"{value}\")");
                }

                remaining = remaining.Remove(m.Index, m.Length);
            }

            // Any remaining terms should be made prefix terms or fuzzy terms
            terms
                .AddRange(remaining.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(EscapeSearchText)
                .SelectMany(t => new[] { $"{t}*", $"{t}~" }));

            return string.Join(" || ", terms);

            string CombineWords(string text, string sep) =>
                string.Join(sep, text.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            string EscapeSearchText(string text)
            {
                if (text.Equals("and", StringComparison.OrdinalIgnoreCase))
                {
                    return "\\and";
                }
                else if (text.Equals("or", StringComparison.OrdinalIgnoreCase))
                {
                    return "\\or";
                }

                var sb = new StringBuilder();

                foreach (var c in text)
                {
                    if (_luceneSyntaxEscapeChars.Contains(c))
                    {
                        sb.Append("\\");
                    }

                    sb.Append(c);
                }

                return sb.ToString();
            }
        }

        private (int limit, int start) ResolvePagingParams(int? limit, int? start)
        {
            if (limit.HasValue)
            {
                if (limit.Value <= 0)
                {
                    throw new ProblemDetailsException(
                        new ProblemDetails()
                        {
                            Detail = "limit parameter is invalid.",
                            Status = 400,
                            Title = "InvalidPagingParameters"
                        });
                }
                else if (limit.Value > _settings.MaxTop)
                {
                    throw new ProblemDetailsException(
                        new ProblemDetails()
                        {
                            Detail = $"limit parameter cannot be greater than {_settings.MaxTop}.",
                            Status = 400,
                            Title = "InvalidPagingParameters"
                        });
                }
            }

            if (start.HasValue && start.Value < 0)
            {
                throw new ProblemDetailsException(
                    new ProblemDetails()
                    {
                        Detail = "start parameter is invalid.",
                        Status = 400,
                        Title = "InvalidPagingParameters"
                    });
            }

            var top = limit ?? _settings.DefaultTop;
            var skip = start ?? 0;

            return (top, skip);
        }

        private async Task<(float lat, float lng)?> TryGetCoordinatesForPostcode(string postcode)
        {
            var parameters = new SearchParameters
            {
                SearchFields = new[] { "pcds" },
                Select = new[] { "pcds", "lat", "long" },
                SearchMode = SearchMode.All,
                Top = 1,
                QueryType = QueryType.Full
            };
            var results = await _onspdIndex.Documents.SearchAsync<dynamic>(postcode, parameters);

            if (results.Results.Count > 0)
            {
                return ((float)results.Results.First().Document.lat, (float)results.Results.First().Document.@long);
            }
            else
            {
                return null;
            }
        }
    }
}
