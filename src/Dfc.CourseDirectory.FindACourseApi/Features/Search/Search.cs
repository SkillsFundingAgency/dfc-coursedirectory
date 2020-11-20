using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using Dfc.CourseDirectory.Core.Validation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using FindACourseOffering = Dfc.CourseDirectory.Core.Search.Models.FindACourseOffering;

namespace Dfc.CourseDirectory.FindACourseApi.Features.Search
{
    public class Query : IRequest<OneOf<ProblemDetails, SearchViewModel>>
    {
        public string SubjectKeyword { get; set; }
        public float? Distance { get; set; }
        public string ProviderName { get; set; }
        public IEnumerable<string> QualificationLevels { get; set; }
        public IEnumerable<int> StudyModes { get; set; }
        public IEnumerable<int> AttendancePatterns { get; set; }
        public IEnumerable<int> DeliveryModes { get; set; }
        public string Town { get; set; }
        public string Postcode { get; set; }
        public SearchSortBy? SortBy { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public int? Limit { get; set; }
        public int? Start { get; set; }
    }

    public class Handler : IRequestHandler<Query, OneOf<ProblemDetails, SearchViewModel>>
    {
        private const int DefaultSize = 20;
        private const int MaxSize = 50;

        private static readonly IReadOnlyDictionary<string, string> _courseSearchFacetMapping = new Dictionary<string, string>()
        {
            { "NotionalNVQLevelv2", "QualificationLevel" },
            { "VenueStudyMode", "StudyMode" },
            { "VenueAttendancePattern", "AttendancePattern" }
        };

        private static readonly HashSet<char> _luceneSyntaxEscapeChars = new HashSet<char>("+-&|!(){}[]^\"~*?:\\/");

        private readonly ISearchClient<FindACourseOffering> _courseSearchClient;
        private readonly ISearchClient<Onspd> _onspdSearchClient;

        public Handler(
            ISearchClient<FindACourseOffering> courseSearchClient,
            ISearchClient<Onspd> onspdSearchClient)
        {
            _courseSearchClient = courseSearchClient;
            _onspdSearchClient = onspdSearchClient;
        }

        public async Task<OneOf<ProblemDetails, SearchViewModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            var filters = new List<string>();

            if (request.SortBy == SearchSortBy.Distance && string.IsNullOrWhiteSpace(request.Postcode))
            {
                return new ProblemDetails()
                {
                    Detail = "Postcode is required to sort by Distance.",
                    Status = 400,
                    Title = "PostcodeRequired"
                };
            }

            var gotPostcode = !string.IsNullOrWhiteSpace(request.Postcode);

            if (gotPostcode)
            {
                if (!Rules.UkPostcodePattern.IsMatch(request.Postcode))
                {
                    return new ProblemDetails()
                    {
                        Detail = "Postcode is not valid.",
                        Status = 400,
                        Title = "InvalidPostcode"
                    };
                }
            }

            var geoFilterRequired = request.Distance.GetValueOrDefault(0) > 0 && gotPostcode;

            // lat/lng required if Distance filter is specified *or* sorting by Distance
            var getPostcodeCoords = geoFilterRequired || request.SortBy == SearchSortBy.Distance;
            float? latitude = null;
            float? longitude = null;
            if (getPostcodeCoords)
            {
                var coords = await TryGetCoordinatesForPostcode(request.Postcode);

                if (!coords.HasValue)
                {
                    return new ProblemDetails()
                    {
                        Detail = "Specified postcode cannot be found.",
                        Status = 400,
                        Title = "PostcodeNotFound"
                    };
                }

                latitude = coords.Value.lat;
                longitude = coords.Value.lng;
            }

            if (request.StartDateFrom.HasValue)
            {
                filters.Add($"StartDate ge {request.StartDateFrom.Value:o}");
            }

            if (request.StartDateTo.HasValue)
            {
                filters.Add($"StartDate le {request.StartDateTo.Value:o}");
            }

            if (request.AttendancePatterns?.Any() ?? false)
            {
                // TODO Validate AttendancePatterns? Consider using enum instead of int

                filters.Add($"search.in({nameof(FindACourseOffering.VenueAttendancePattern)}, '{string.Join("|", request.AttendancePatterns)}', '|')");
            }

            if (request.QualificationLevels?.Any() ?? false)
            {
                // TODO Validate QualificationLevels?

                filters.Add($"search.in({nameof(FindACourseOffering.NotionalNVQLevelv2)}, '{string.Join("|", request.QualificationLevels.Select(EscapeFilterValue))}', '|')");
            }

            if (geoFilterRequired)
            {
                var distanceInKm = GeoHelper.MilesToKilometers(request.Distance.Value);

                filters.Add(
                    $"(geo.distance({nameof(FindACourseOffering.VenueLocation)}, geography'POINT({longitude.Value} {latitude.Value})') le {distanceInKm}" +
                    $" or {nameof(FindACourseOffering.National)} eq true" +
                    $" or {nameof(FindACourseOffering.DeliveryMode)} eq '2')");
            }

            if (!string.IsNullOrWhiteSpace(request.Town))
            {
                filters.Add($"search.ismatch('{EscapeFilterValue(request.Town)}', '{nameof(FindACourseOffering.VenueTown)}')");
            }

            if (request.StudyModes?.Any() ?? false)
            {
                filters.Add($"search.in({nameof(FindACourseOffering.VenueStudyMode)}, '{string.Join("|", request.StudyModes)}', '|')");
            }

            if (request.DeliveryModes?.Any() ?? false)
            {
                filters.Add($"search.in({nameof(FindACourseOffering.DeliveryMode)}, '{string.Join("|", request.DeliveryModes)}', '|')");
            }

            if (!string.IsNullOrWhiteSpace(request.ProviderName))
            {
                filters.Add($"search.ismatchscoring('{EscapeFilterValue(request.ProviderName)}', 'ProviderName', 'simple', 'any')");
            }

            var orderBy = request.SortBy == SearchSortBy.StartDateDescending ?
                "StartDate desc" : request.SortBy == SearchSortBy.StartDateAscending ?
                "StartDate asc" : request.SortBy == SearchSortBy.Distance ?
                $"geo.distance(VenueLocation, geography'POINT({longitude.Value} {latitude.Value})')" :
                "search.score() desc";

            if (!TryResolvePagingParams(request.Limit, request.Start, out var size, out var skip, out var problem))
            {
                return problem;
            }

            var searchText = TranslateCourseSearchSubjectText(request.SubjectKeyword);

            var query = new CourseSearchQuery()
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
                Filters = filters,
                CourseName = searchText,
                Size = size,
                Skip = skip,
                OrderBy = orderBy
            };

            var result = await _courseSearchClient.Search(query);

            return new SearchViewModel()
            {
                Limit = size,
                Start = skip,
                Total = Convert.ToInt32(result.TotalCount.Value),
                Facets = result.Facets.ToDictionary(
                    f => _courseSearchFacetMapping.GetValueOrDefault(f.Key, f.Key),
                    f => f.Value.Select(v => new FacetCountResultViewModel()
                    {
                        Value = v.Key,
                        Count = v.Value.Value
                    })),
                Results = result.Items.Select(i => new SearchResultViewModel()
                {
                    Cost = i.Record.Cost,
                    CostDescription = i.Record.CostDescription,
                    CourseDescription = i.Record.CourseDescription,
                    CourseName = i.Record.CourseName,
                    CourseId = i.Record.CourseId,
                    CourseRunId = i.Record.CourseRunId,
                    CourseText = i.Record.CourseText,
                    DeliveryMode = i.Record.DeliveryMode,
                    DeliveryModeDescription = Enum.Parse<CourseDeliveryMode>(i.Record.DeliveryMode).ToDescription(),
                    Distance = GetDistanceFromPostcodeForResult(i),
                    DurationUnit = i.Record.DurationUnit ?? 0,
                    DurationValue = i.Record.DurationValue,
                    FlexibleStartDate = i.Record.FlexibleStartDate,
                    LearnAimRef = i.Record.LearnAimRef,
                    National = i.Record.National,
                    QualificationLevel = i.Record.NotionalNVQLevelv2,
                    OfferingType = i.Record.OfferingType,
                    ProviderName = i.Record.ProviderName,
                    QualificationCourseTitle = i.Record.QualificationCourseTitle,
                    Region = i.Record.Region,
                    SearchScore = i.Score.Value,
                    StartDate = !i.Record.FlexibleStartDate.GetValueOrDefault() ? i.Record.StartDate : null,
                    Ukprn = i.Record.UKPRN.ToString(),
                    UpdatedOn = i.Record.UpdatedOn,
                    VenueAddress = i.Record.VenueAddress,
                    VenueAttendancePattern = i.Record.VenueAttendancePattern,
                    VenueAttendancePatternDescription = Enum.Parse<CourseDeliveryMode>(i.Record.DeliveryMode) == CourseDeliveryMode.ClassroomBased ?
                        Enum.Parse<CourseAttendancePattern>(i.Record.VenueAttendancePattern).ToDescription() :
                        null,
                    VenueLocation = i.Record.VenueLocation != null ?
                        new CoordinatesViewModel()
                        {
                            Latitude = i.Record.VenueLocation.Latitude,
                            Longitude = i.Record.VenueLocation.Longitude
                        } :
                        null,
                    VenueName = i.Record.VenueName,
                    VenueStudyMode = i.Record.VenueStudyMode,
                    VenueStudyModeDescription = Enum.Parse<CourseStudyMode>(i.Record.VenueStudyMode).ToDescription(),
                    VenueTown = i.Record.VenueTown
                }).ToList()
            };

            double? GetDistanceFromPostcodeForResult(SearchResultItem<FindACourseOffering> item) =>
                getPostcodeCoords && item.Record.VenueLocation != null && item.Record.National != true ?
                    Math.Round(
                        GeoHelper.KilometersToMiles(
                            GeoHelper.GetDistanceTo(
                                (latitude.Value, longitude.Value),
                                (item.Record.VenueLocation.Latitude, item.Record.VenueLocation.Longitude))),
                        2) :
                    (double?)null;
        }

        private static string EscapeFilterValue(string v) => v.Replace("'", "''");

        private static bool TryResolvePagingParams(int? limit, int? start, out int size, out int skip, out ProblemDetails problem)
        {
            if (limit.HasValue)
            {
                if (limit.Value <= 0)
                {
                    return Problem(out size, out skip, out problem, "InvalidPagingParameters", "limit parameter is invalid.");
                }
                else if (limit.Value > MaxSize)
                {
                    return Problem(out size, out skip, out problem, "InvalidPagingParameters", $"limit parameter cannot be greater than {MaxSize}.");
                }
            }

            if (start.HasValue && start.Value < 0)
            {
                return Problem(out size, out skip, out problem, "InvalidPagingParameters", "Start parameter is invalid.");
            }

            size = limit ?? DefaultSize;
            skip = start ?? 0;
            problem = null;
            return true;

            bool Problem(out int size, out int skip, out ProblemDetails problem, string title, string detail)
            {
                size = 0;
                skip = 0;
                problem = new ProblemDetails
                {
                    Title = title,
                    Detail = detail,
                    Status = 400
                };
                return false;
            }
        }

        private static string TranslateCourseSearchSubjectText(string subjectText)
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

        private async Task<(float lat, float lng)?> TryGetCoordinatesForPostcode(string postcode)
        {
            var result = await _onspdSearchClient.Search(new OnspdSearchQuery() { Postcode = postcode });

            if (result.TotalCount != 0)
            {
                var doc = result.Items.Single();
                return ((float)doc.Record.lat, (float)doc.Record.@long);
            }
            else
            {
                return null;
            }
        }
    }
}
