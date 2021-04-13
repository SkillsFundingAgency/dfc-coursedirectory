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
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
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
            { "RegionName", "Region" },
            { "ProviderDisplayName", "ProviderName" }
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

            // If either lat or lng is specified then both must be specified
            if (request.Latitude.HasValue != request.Longitude.HasValue)
            {
                return new ProblemDetails()
                {
                    Detail = "Latitude & longitude must both be specified.",
                    Status = 400,
                    Title = "InvalidLatLng"
                };
            }

            if (request.SortBy == SearchSortBy.Distance &&
                (string.IsNullOrWhiteSpace(request.Postcode) && (!request.Latitude.HasValue || !request.Longitude.HasValue)))
            {
                return new ProblemDetails()
                {
                    Detail = "Postcode is required to sort by Distance.",
                    Status = 400,
                    Title = "PostcodeRequired"
                };
            }

            Postcode postcode = null;

            if (!string.IsNullOrWhiteSpace(request.Postcode))
            {
                if (!Postcode.TryParse(request.Postcode, out postcode))
                {
                    return new ProblemDetails()
                    {
                        Detail = "Postcode is not valid.",
                        Status = 400,
                        Title = "InvalidPostcode"
                    };
                }
            }

            var geoFilterRequired = request.Distance.GetValueOrDefault(0) > 0 &&
                (postcode != null || (request.Latitude.HasValue && request.Longitude.HasValue));

            // lat/lng required if Distance filter is specified *or* sorting by Distance
            var getPostcodeCoords = (geoFilterRequired || request.SortBy == SearchSortBy.Distance) && !request.Latitude.HasValue;
            double? latitude = request.Latitude;
            double? longitude = request.Longitude;
            if (getPostcodeCoords)
            {
                var coords = await TryGetCoordinatesForPostcode(postcode);

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

                filters.Add($"({string.Join(" or ", request.AttendancePatterns.Select(ap => $"{nameof(FindACourseOffering.AttendancePattern)} eq {ap}"))})");
            }

            if (request.QualificationLevels?.Any() ?? false)
            {
                // TODO Validate QualificationLevels?

                filters.Add($"search.in({nameof(FindACourseOffering.NotionalNVQLevelv2)}, '{string.Join("|", request.QualificationLevels.Select(EscapeFilterValue))}', '|')");
            }

            if (geoFilterRequired)
            {
                var distanceInKm = Convert.ToDecimal(GeoHelper.MilesToKilometers(request.Distance.Value));

                filters.Add(
                    $"(geo.distance({nameof(FindACourseOffering.Position)}, geography'POINT({longitude.Value} {latitude.Value})') le {distanceInKm}" +
                    $" or {nameof(FindACourseOffering.National)} eq true" +
                    $" or {nameof(FindACourseOffering.DeliveryMode)} eq 2)");
            }

            if (!string.IsNullOrWhiteSpace(request.Town))
            {
                filters.Add($"search.ismatch('{EscapeFilterValue(request.Town)}', '{nameof(FindACourseOffering.VenueTown)}')");
            }

            if (request.StudyModes?.Any() ?? false)
            {
                filters.Add($"({string.Join(" or ", request.StudyModes.Select(sm => $"{nameof(FindACourseOffering.StudyMode)} eq {sm}"))})");
            }

            if (request.DeliveryModes?.Any() ?? false)
            {
                filters.Add($"({string.Join(" or ", request.DeliveryModes.Select(dm => $"{nameof(FindACourseOffering.DeliveryMode)} eq {dm}"))})");
            }

            if (!string.IsNullOrWhiteSpace(request.ProviderName))
            {
                filters.Add($"search.ismatchscoring('{EscapeFilterValue(request.ProviderName)}', '{nameof(FindACourseOffering.ProviderDisplayName)}', 'simple', 'any')");
            }

            var orderBy = request.SortBy == SearchSortBy.StartDateDescending ?
                "StartDate desc" : request.SortBy == SearchSortBy.StartDateAscending ?
                "StartDate asc" : request.SortBy == SearchSortBy.Distance ?
                $"geo.distance({nameof(FindACourseOffering.Position)}, geography'POINT({longitude.Value} {latitude.Value})')" :
                "search.score() desc";

            if (!TryResolvePagingParams(request.Limit, request.Start, out var size, out var skip, out var problem))
            {
                return problem;
            }

            var searchText = TranslateCourseSearchSubjectText(request.SubjectKeyword);

            var query = new FindACourseOfferingSearchQuery()
            {
                Facets = new[]
                {
                    "NotionalNVQLevelv2,count:100",
                    "StudyMode",
                    "AttendancePattern",
                    "DeliveryMode",
                    "ProviderDisplayName,count:100",
                    "RegionName,count:100"
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
                        Value = v.Key.ToString(),
                        Count = v.Value.Value
                    })),
                Results = result.Items.Select(i => new SearchResultViewModel()
                {
                    Cost = !string.IsNullOrEmpty(i.Record.Cost) ?
                        Convert.ToInt32(decimal.Parse(i.Record.Cost)) :
                        (int?)null,
                    CostDescription = i.Record.CostDescription,
                    CourseDescription = i.Record.CourseDescription,
                    CourseName = i.Record.CourseName,
                    CourseId = i.Record.CourseId,
                    CourseRunId = i.Record.CourseRunId,
                    CourseText = i.Record.CourseDescription,
                    DeliveryMode = ((int)i.Record.DeliveryMode).ToString(),
                    DeliveryModeDescription = i.Record.DeliveryMode.ToDescription(),
                    Distance = GetDistanceFromPostcodeForResult(i),
                    DurationUnit = i.Record.DurationUnit ?? 0,
                    DurationValue = i.Record.DurationValue,
                    FlexibleStartDate = i.Record.FlexibleStartDate,
                    LearnAimRef = i.Record.LearnAimRef,
                    National = i.Record.National,
                    QualificationLevel = i.Record.NotionalNVQLevelv2,
                    OfferingType = i.Record.OfferingType,
                    ProviderName = i.Record.ProviderDisplayName,
                    QualificationCourseTitle = i.Record.QualificationCourseTitle,
                    Region = i.Record.RegionName,
                    SearchScore = i.Score.Value,
                    StartDate = !i.Record.FlexibleStartDate.GetValueOrDefault() ? i.Record.StartDate : null,
                    TLevelId = i.Record.TLevelId,
                    TLevelLocationId = i.Record.TLevelLocationId,
                    Ukprn = i.Record.ProviderUkprn.ToString(),
                    UpdatedOn = i.Record.UpdatedOn,
                    VenueAddress = i.Record.VenueAddress,
                    VenueAttendancePattern = ((int?)i.Record.AttendancePattern)?.ToString(),
                    VenueAttendancePatternDescription = i.Record.DeliveryMode == CourseDeliveryMode.ClassroomBased ?
                        i.Record.AttendancePattern?.ToDescription() :
                        null,
                    VenueLocation = i.Record.Position != null ?
                        new CoordinatesViewModel()
                        {
                            Latitude = i.Record.Position.Latitude,
                            Longitude = i.Record.Position.Longitude
                        } :
                        null,
                    VenueName = i.Record.VenueName,
                    VenueStudyMode = ((int)i.Record.StudyMode).ToString(),
                    VenueStudyModeDescription = i.Record.StudyMode.ToDescription(),
                    VenueTown = i.Record.VenueTown
                }).ToList()
            };

            double? GetDistanceFromPostcodeForResult(SearchResultItem<FindACourseOffering> item) =>
                getPostcodeCoords && item.Record.Position != null && item.Record.National != true ?
                    Math.Round(
                        GeoHelper.KilometersToMiles(
                            GeoHelper.GetDistanceTo(
                                (latitude.Value, longitude.Value),
                                (item.Record.Position.Latitude, item.Record.Position.Longitude))),
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
            if (string.IsNullOrWhiteSpace(subjectText) || subjectText.Trim() == "*")
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
                .Where(term => !(term.Length == 1 && _luceneSyntaxEscapeChars.Contains(term[0])))
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

        private async Task<(float lat, float lng)?> TryGetCoordinatesForPostcode(Postcode postcode)
        {
            var result = await _onspdSearchClient.Search(new OnspdSearchQuery() { Postcode = postcode });

            var doc = result.Items.SingleOrDefault();

            if (doc != null)
            {
                return ((float)doc.Record.lat, (float)doc.Record.@long);
            }

            return null;
        }
    }
}
