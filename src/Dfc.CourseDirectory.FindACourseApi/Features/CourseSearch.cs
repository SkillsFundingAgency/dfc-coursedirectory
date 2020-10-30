using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.FindACourseApi.DTOs;
using Dfc.CourseDirectory.FindACourseApi.Interfaces;
using Dfc.CourseDirectory.FindACourseApi.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.FindACourseApi.Features.CourseSearch
{
    public class Query : IRequest<ViewModel>
    {
        public string SubjectKeyword { get; set; }
        public float? Distance { get; set; }
        public string ProviderName { get; set; }
        public string[] QualificationLevels { get; set; }
        public int[] StudyModes { get; set; }
        public int[] AttendancePatterns { get; set; }
        public int[] DeliveryModes { get; set; }
        public string Town { get; set; }
        public string Postcode { get; set; }
        public CourseSearchSortBy? SortBy { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public int? Limit { get; set; }
        public int? Start { get; set; }
    }

    public class ViewModel
    {
        public IReadOnlyDictionary<string, IEnumerable<FacetCountResult>> Facets { get; set; }
        public IReadOnlyCollection<CourseSearchResult> Results { get; set; }
        public int Total { get; set; }
        public int Limit { get; set; }
        public int Start { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
    {
        private static readonly IReadOnlyDictionary<string, string> _courseSearchFacetMapping = new Dictionary<string, string>()
        {
            { "NotionalNVQLevelv2", "QualificationLevel" },
            { "VenueStudyMode", "StudyMode" },
            { "VenueAttendancePattern", "AttendancePattern" }
        };

        private readonly ICourseService _courseService;
        private readonly ILogger _logger;

        public Handler(ICourseService courseService, ILoggerFactory loggerFactory)
        {
            _courseService = courseService;
            _logger = loggerFactory.CreateLogger<Handler>();
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var criteria = new SearchCriteriaStructure()
            {
                AttendancePatterns = request.AttendancePatterns,
                DeliveryModes = request.DeliveryModes,
                Distance = request.Distance,
                Limit = request.Limit,
                Postcode = request.Postcode,
                ProviderName = request.ProviderName,
                QualificationLevels = request.QualificationLevels,
                SortBy = request.SortBy,
                Start = request.Start,
                StartDateFrom = request.StartDateFrom,
                StartDateTo = request.StartDateTo,
                StudyModes = request.StudyModes,
                SubjectKeyword = request.SubjectKeyword,
                Town = request.Town
            };

            var result = await _courseService.CourseSearch(_logger, criteria);

            return new ViewModel()
            {
                Limit = result.Limit,
                Start = result.Start,
                Total = result.Total,
                Facets = result.Facets.ToDictionary(
                    f => _courseSearchFacetMapping.GetValueOrDefault(f.Key, f.Key),
                    f => f.Value.Select(v => new FacetCountResult()
                    {
                        Value = v.Value,
                        Count = v.Count.Value
                    })),
                Results = result.Items.Select(i => new CourseSearchResult()
                {
                    Cost = i.Course.Cost,
                    CostDescription = i.Course.CostDescription,
                    CourseDescription = i.Course.CourseDescription,
                    CourseName = i.Course.CourseName,
                    CourseId = i.Course.CourseId,
                    CourseRunId = i.Course.CourseRunId,
                    CourseText = i.Course.CourseText,
                    DeliveryMode = i.Course.DeliveryMode,
                    DeliveryModeDescription = i.Course.DeliveryModeDescription,
                    Distance = !i.Course.National.GetValueOrDefault() ? i.Distance : null,
                    DurationUnit = i.Course.DurationUnit ?? DurationUnit.Undefined,
                    DurationValue = i.Course.DurationValue,
                    FlexibleStartDate = i.Course.FlexibleStartDate,
                    LearnAimRef = i.Course.LearnAimRef,
                    National = i.Course.National,
                    QualificationLevel = i.Course.NotionalNVQLevelv2,
                    ProviderName = i.Course.ProviderName,
                    QualificationCourseTitle = i.Course.QualificationCourseTitle,
                    Region = i.Course.Region,
                    SearchScore = i.Score,
                    StartDate = !i.Course.FlexibleStartDate.GetValueOrDefault() ? i.Course.StartDate : null,
                    UKPRN = i.Course.UKPRN,
                    UpdatedOn = i.Course.UpdatedOn,
                    VenueAddress = i.Course.VenueAddress,
                    VenueAttendancePattern = i.Course.VenueAttendancePattern,
                    VenueAttendancePatternDescription = i.Course.VenueAttendancePatternDescription,
                    VenueLocation = i.Course.VenueLocation != null ?
                        new Coordinates()
                        {
                            Latitude = i.Course.VenueLocation.Latitude,
                            Longitude = i.Course.VenueLocation.Longitude
                        } :
                        null,
                    VenueName = i.Course.VenueName,
                    VenueStudyMode = i.Course.VenueStudyMode,
                    VenueStudyModeDescription = i.Course.VenueStudyModeDescription,
                    VenueTown = i.Course.VenueTown
                }).ToList()
            };
        }
    }
}
