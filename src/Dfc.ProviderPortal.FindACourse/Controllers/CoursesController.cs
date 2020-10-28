using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.ProviderPortal.FindACourse.ApiModels;
using Dfc.ProviderPortal.FindACourse.Interfaces;
using Dfc.ProviderPortal.FindACourse.Models;
using Dfc.ProviderPortal.Packages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Dfc.ProviderPortal.FindACourse.Controllers
{
    [ApiController]
    public class CoursesController : ControllerBase, IActionFilter
    {
        private static readonly Dictionary<string, string> _courseSearchFacetMapping = new Dictionary<string, string>()
        {
            { "NotionalNVQLevelv2", "QualificationLevel" },
            { "VenueStudyMode", "StudyMode" },
            { "VenueAttendancePattern", "AttendancePattern" }
        };

        private readonly ILogger _log;
        private readonly ICourseService _service;

        public CoursesController(
            ILogger<CoursesController> logger,
            ICourseService service)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(service, nameof(service));

            _log = logger;
            _service = service;
        }

        [Route("~/coursesearch")]
        [HttpPost]
        [ProducesResponseType(typeof(CourseSearchResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CourseSearch([FromBody]CourseSearchRequest request)
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

            var result = await _service.CourseSearch(_log, criteria);

            var response = new CourseSearchResponse()
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
                Results = result.Items.Select(i => new CourseSearchResponseItem()
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

            return new OkObjectResult(response);
        }

        [Route("~/courserundetail")]
        [HttpGet]
        [ProducesResponseType(typeof(CourseRunDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CourseRunDetail([FromQuery] CourseRunDetailRequest request)
        {
            var result = await _service.CourseDetail(request.CourseId, request.CourseRunId);

            if (result != null)
            {
                var courseRun = result.Course.CourseRuns.Single(r => r.id == request.CourseRunId);
                var venue = courseRun.VenueId.HasValue ? result.CourseRunVenues.Single(v => v.id == courseRun.VenueId) : null;
                var providerContact = ((JArray)result.Provider.ProviderContact)
                    .Select(t => t.ToObject<Providercontact>())
                    .SingleOrDefault(c => c.ContactType == "P");

                var alternativeCourseRuns = result.Course.CourseRuns.Where(r => r.id != request.CourseRunId)
                    .Where(r => r.RecordStatus == RecordStatus.Live)
                    .Select(r => new { CourseRun = r, Venue = result.CourseRunVenues.SingleOrDefault(v => v.id == r.VenueId) });

                var response = new CourseRunDetailResponse()
                {
                    CourseRunId = courseRun.id,
                    AttendancePattern = courseRun.DeliveryMode == DeliveryMode.ClassroomBased ? (AttendancePattern?)courseRun.AttendancePattern : null,
                    Cost = courseRun.Cost,
                    CostDescription = courseRun.CostDescription,
                    CourseName = courseRun.CourseName,
                    CourseURL = courseRun.CourseURL,
                    CreatedDate = courseRun.CreatedDate,
                    DeliveryMode = courseRun.DeliveryMode,
                    DurationUnit = courseRun.DurationUnit,
                    DurationValue = courseRun.DurationValue,
                    FlexibleStartDate = courseRun.FlexibleStartDate,
                    StartDate = !courseRun.FlexibleStartDate ? courseRun.StartDate : null,
                    StudyMode = courseRun.StudyMode,
                    National = courseRun.National,
                    Course = new CourseDetailResponseCourse()
                    {
                        AdvancedLearnerLoan = result.Course.AdvancedLearnerLoan,
                        AwardOrgCode = result.Course.AwardOrgCode,
                        CourseDescription = result.Course.CourseDescription,
                        CourseId = result.Course.id,
                        EntryRequirements = result.Course.EntryRequirements,
                        HowYoullBeAssessed = result.Course.HowYoullBeAssessed,
                        HowYoullLearn = result.Course.HowYoullLearn,
                        LearnAimRef = result.Course.LearnAimRef,
                        QualificationLevel = result.Course.NotionalNVQLevelv2,
                        WhatYoullLearn = result.Course.WhatYoullLearn,
                        WhatYoullNeed = result.Course.WhatYoullNeed,
                        WhereNext = result.Course.WhereNext
                    },
                    Venue = venue != null ?
                        new CourseDetailResponseVenue()
                        {
                            AddressLine1 = venue.ADDRESS_1,
                            AddressLine2 = venue.ADDRESS_2,
                            County = venue.COUNTY,
                            Email = venue.Email,
                            Postcode = venue.POSTCODE,
                            Telephone = venue.Telephone,
                            Town = venue.TOWN,
                            VenueName = venue.VENUE_NAME,
                            Website = UrlUtil.EnsureHttpPrefixed(venue.WEBSITE),
                            Latitude = venue.Latitude,
                            Longitude = venue.Longitude
                        } :
                        null,
                    Provider = new CourseDetailResponseProvider()
                    {
                        ProviderName = result.Provider.ProviderName,
                        TradingName = result.Provider.TradingName,
                        CourseDirectoryName = result.Provider.CourseDirectoryName,
                        Alias = result.Provider.Alias,
                        UKPRN = result.Provider.UnitedKingdomProviderReferenceNumber,
                        AddressLine1 = providerContact?.ContactAddress?.SAON?.Description,
                        AddressLine2 = providerContact?.ContactAddress?.PAON?.Description,
                        Town = providerContact?.ContactAddress?.Items?.FirstOrDefault()?.ToString(),
                        Postcode = providerContact?.ContactAddress?.PostCode,
                        County = providerContact?.ContactAddress?.Locality,
                        Telephone = providerContact?.ContactTelephone1,
                        Fax = providerContact?.ContactFax,
                        Website = UrlUtil.EnsureHttpPrefixed(providerContact?.ContactWebsiteAddress),
                        Email = providerContact?.ContactEmail,
                        EmployerSatisfaction = result.FeChoice?.EmployerSatisfaction,
                        LearnerSatisfaction = result.FeChoice?.LearnerSatisfaction,
                    },
                    Qualification = new CourseDetailResponseQualification()
                    {
                        AwardOrgCode = result.Qualification.AwardOrgCode,
                        AwardOrgName = result.Qualification.AwardOrgName,
                        LearnAimRef = result.Qualification.LearnAimRef,
                        LearnAimRefTitle = result.Qualification.LearnAimRefTitle,
                        LearnAimRefTypeDesc = result.Qualification.LearnAimRefTypeDesc,
                        QualificationLevel = result.Qualification.NotionalNVQLevelv2,
                        SectorSubjectAreaTier1Desc = result.Qualification.SectorSubjectAreaTier1Desc,
                        SectorSubjectAreaTier2Desc = result.Qualification.SectorSubjectAreaTier2Desc
                    },
                    AlternativeCourseRuns = alternativeCourseRuns.Select(ar => new CourseDetailResponseAlternativeCourseRun()
                    {
                        CourseRunId = ar.CourseRun.id,
                        AttendancePattern = ar.CourseRun.DeliveryMode == DeliveryMode.ClassroomBased ? (AttendancePattern?)ar.CourseRun.AttendancePattern : null,
                        Cost = ar.CourseRun.Cost,
                        CostDescription = ar.CourseRun.CostDescription,
                        CourseName = ar.CourseRun.CourseName,
                        CourseURL = ar.CourseRun.CourseURL,
                        CreatedDate = ar.CourseRun.CreatedDate,
                        DeliveryMode = ar.CourseRun.DeliveryMode,
                        DurationUnit = ar.CourseRun.DurationUnit,
                        DurationValue = ar.CourseRun.DurationValue,
                        FlexibleStartDate = ar.CourseRun.FlexibleStartDate,
                        StartDate = !ar.CourseRun.FlexibleStartDate ? ar.CourseRun.StartDate : null,
                        StudyMode = ar.CourseRun.StudyMode,
                        Venue = ar.Venue != null ?
                            new CourseDetailResponseVenue()
                            {
                                AddressLine1 = ar.Venue.ADDRESS_1,
                                AddressLine2 = ar.Venue.ADDRESS_2,
                                County = ar.Venue.COUNTY,
                                Email = ar.Venue.Email,
                                Postcode = ar.Venue.POSTCODE,
                                Telephone = ar.Venue.Telephone,
                                Town = ar.Venue.TOWN,
                                VenueName = ar.Venue.VENUE_NAME,
                                Website = UrlUtil.EnsureHttpPrefixed(ar.Venue.WEBSITE),
                                Latitude = ar.Venue.Latitude,
                                Longitude = ar.Venue.Longitude
                            } :
                            null
                    }).ToList(),
                    SubRegions = (from region in RegionInfo.All
                                 from subRegion in region.SubRegions
                                 let sr = new { subRegion, region }
                                 join r in (courseRun.Regions ?? Enumerable.Empty<string>()) on sr.subRegion.Id equals r
                                 select new CourseDetailResponseSubRegion()
                                 {
                                     SubRegionId = r,
                                     Name = sr.subRegion.Name,
                                     ParentRegion = new CourseDetailResponseRegion()
                                     {
                                         Name = sr.region.Name,
                                         RegionId = sr.region.Id
                                     }
                                 }).ToList()
                };

                return new OkObjectResult(response);
            }
            else
            {
                return new NotFoundResult();
            }
        }

        [Route("~/providersearch")]
        [HttpPost]
        [ProducesResponseType(typeof(ProviderSearchResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ProviderSearch([FromBody]ProviderSearchRequest request)
        {
            var criteria = new ProviderSearchCriteriaStructure()
            {
                Keyword = request.Keyword,
                Region = request.Region,
                TopResults = request.TopResults,
                Town = request.Town
            };

            _log.LogInformation($"Provider search with keyword {request.Keyword}");
            var result = await _service.ProviderSearch(_log, criteria);

            var response = new ProviderSearchResponse()
            {
                ODataContext = result.ODataContext,
                ODataCount = result.ODataCount,
                SearchFacets = result.SearchFacets,
                Value = result.Value.Select(i => new ProviderSearchResponseItem()
                {
                    CourseDirectoryName = i.CourseDirectoryName,
                    id = i.id,
                    Name = i.Name,
                    Postcode = i.Postcode,
                    ProviderAlias = i.ProviderAlias,
                    ProviderStatus = i.ProviderStatus,
                    Region = i.Region,
                    Status = i.Status,
                    Town = i.Town,
                    TradingName = i.TradingName,
                    UKPRN = i.UKPRN
                })
            };

            return new OkObjectResult(response);
        }

        [Route("~/providerget")]
        [HttpPost]
        [ProducesResponseType(typeof(ProviderGetResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ProviderGet([FromBody]ProviderGetRequest request)
        {
            _log.LogInformation($"FAC ProviderGet called for PRN {request.UKPRN}");
            var result = await _service.ProviderDetail(request.UKPRN);

            if (result != null)
            {
                var response = new ProviderGetResponse()
                {
                    Alias = result.Alias,
                    BulkUploadStatus = result.BulkUploadStatus,
                    CourseDirectoryName = result.CourseDirectoryName,
                    ExpiryDateSpecified = result.ExpiryDateSpecified,
                    id = result.id,
                    MarketingInformation = result.MarketingInformation,
                    NationalApprenticeshipProvider = result.NationalApprenticeshipProvider,
                    ProviderAliases = result.ProviderAliases,
                    ProviderAssociations = result.ProviderAssociations,
                    ProviderContact = result.ProviderContact,
                    ProviderId = result.ProviderId,
                    ProviderName = result.ProviderName,
                    ProviderStatus = result.ProviderStatus,
                    ProviderType = result.ProviderType,
                    ProviderVerificationDate = result.ProviderVerificationDate,
                    ProviderVerificationDateSpecified = result.ProviderVerificationDateSpecified,
                    Status = result.Status,
                    TradingName = result.TradingName,
                    UnitedKingdomProviderReferenceNumber = result.UnitedKingdomProviderReferenceNumber,
                    UPIN = result.UPIN,
                    VerificationDetails = result.VerificationDetails
                };

                return new OkObjectResult(response);
            }
            else
            {
                return new NotFoundResult();
            }
        }

        [Route("~/larssearch")]
        [HttpPost]
        [ProducesResponseType(typeof(LARSSearchResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> LARSSearch([FromBody]LARSSearchRequest request)
        {
            var criteria = new LARSSearchCriteriaStructure()
            {
                AwardOrgAimRef = request.AwardOrgAimRef,
                AwardOrgCode = request.AwardOrgCode,
                Keyword = request.Keyword,
                NotionalNVQLevelv2 = request.NotionalNVQLevelv2,
                SectorSubjectAreaTier1 = request.SectorSubjectAreaTier1,
                SectorSubjectAreaTier2 = request.SectorSubjectAreaTier2,
                TopResults = request.TopResults
            };

            _log.LogInformation($"LARS search with keyword {criteria.Keyword}");
            var result = await _service.LARSSearch(_log, criteria);

            var response = new LARSSearchResponse()
            {
                ODataContext = result.ODataContext,
                ODataCount = result.ODataCount,
                SearchFacets = result.SearchFacets,
                Value = result.Value
            };

            return new OkObjectResult(response);
        }

        [Route("~/onspdsearch")]
        [HttpPost]
        [ProducesResponseType(typeof(PostcodeSearchResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ONSPDSearch([FromBody]PostcodeSearchRequest request)
        {
            var criteria = new PostcodeSearchCriteriaStructure()
            {
                Keyword = request.Keyword,
                TopResults = request.TopResults
            };

            var result = await _service.PostcodeSearch(_log, criteria);

            var response = new PostcodeSearchResponse()
            {
                ODataContext = result.ODataContext,
                ODataCount = result.ODataCount,
                SearchFacets = result.SearchFacets,
                Value = result.Value
            };

            return new OkObjectResult(response);
        }

        [NonAction]
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is ProblemDetailsException pde)
            {
                _log.LogInformation(
                    $"Request error on {context.ActionDescriptor.DisplayName}\nTitle: {pde.ProblemDetails.Title}\nDetail: {pde.ProblemDetails.Detail}");

                context.Result = new ObjectResult(pde.ProblemDetails)
                {
                    ContentTypes = new Microsoft.AspNetCore.Mvc.Formatters.MediaTypeCollection()
                    {
                        new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("application/problem+json")
                    },
                    StatusCode = pde.ProblemDetails.Status ?? 400
                };

                context.ExceptionHandled = true;
            }
        }

        [NonAction]
        public void OnActionExecuting(ActionExecutingContext context)
        {
        }
    }
}