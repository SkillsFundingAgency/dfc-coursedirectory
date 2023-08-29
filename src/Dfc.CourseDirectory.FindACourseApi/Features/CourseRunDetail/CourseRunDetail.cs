using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Search;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.FindACourseApi.Features.CourseRunDetail
{
    public class Query : IRequest<OneOf<NotFound, CourseRunDetailViewModel>>
    {
        public Guid CourseId { get; set; }
        public Guid CourseRunId { get; set; }
    }

    public class Handler : IRequestHandler<Query, OneOf<NotFound, CourseRunDetailViewModel>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ISearchClient<Core.Search.Models.Lars> _larsSearchClient;
        private readonly IRegionCache _regionCache;

        public Handler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            ISearchClient<Core.Search.Models.Lars> larsSearchClient,
            IRegionCache regionCache)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher ?? throw new ArgumentNullException(nameof(sqlQueryDispatcher));
            _larsSearchClient = larsSearchClient ?? throw new ArgumentNullException(nameof(larsSearchClient));
            _regionCache = regionCache ?? throw new ArgumentNullException(nameof(regionCache));
        }

        public async Task<OneOf<NotFound, CourseRunDetailViewModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            var course = await _sqlQueryDispatcher.ExecuteQuery(new GetCourse() { CourseId = request.CourseId });

            if (course == null)
            {
                return new NotFound();
            }

            var courseRun = course.CourseRuns.SingleOrDefault(c => c.CourseRunId == request.CourseRunId && c.CourseRunStatus == CourseStatus.Live);

            if (courseRun == null)
            {
                return new NotFound();
            }

            var getProvider = _sqlQueryDispatcher.ExecuteQuery(new GetProviderByUkprn { Ukprn = course.ProviderUkprn });
            var getQualification = _larsSearchClient.Search(new LarsLearnAimRefSearchQuery { LearnAimRef = course.LearnAimRef });

            await Task.WhenAll(getProvider, getQualification);

            var provider = getProvider.Result;
            var qualification = getQualification.Result.Items.SingleOrDefault();

            var getSqlProvider = _sqlQueryDispatcher.ExecuteQuery(new GetProviderById { ProviderId = provider.ProviderId });
            var getProviderVenues = _sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = provider.ProviderId });

            await Task.WhenAll(getSqlProvider, getProviderVenues);

            var sqlProvider = getSqlProvider.Result;
            var venues = getProviderVenues.Result;

            var venue = courseRun.VenueId.HasValue
                ? venues.Single(v => v.VenueId == courseRun.VenueId)
                : null;

            var providerContact = provider.ProviderContact.SingleOrDefault(c => c.ContactType == "P");
            var providerAddressLines = NormalizeAddress(providerContact?.ContactAddress);

            var alternativeCourseRuns = course.CourseRuns
                .Where(r => r.CourseRunId != request.CourseRunId && r.CourseRunStatus == CourseStatus.Live)
                .Select(r => new { CourseRun = r, Venue = venues.SingleOrDefault(v => v.VenueId == r.VenueId) });

            var regions = await _regionCache.GetAllRegions();

            return new CourseRunDetailViewModel
            {
                CourseRunId = courseRun.CourseRunId,
                OfferingType = Core.Search.Models.FindACourseOfferingType.Course,
                AttendancePattern = courseRun.DeliveryMode == CourseDeliveryMode.ClassroomBased ? (CourseAttendancePattern?)courseRun.AttendancePattern : null,
                Cost = courseRun.Cost,
                CostDescription = HtmlEncode(courseRun.CostDescription),
                CourseName = HtmlEncode(courseRun.CourseName),
                CourseURL = ViewModelFormatting.EnsureHttpPrefixed(courseRun.CourseWebsite),
                CreatedDate = courseRun.CreatedOn,
                DeliveryMode = courseRun.DeliveryMode,
                DurationUnit = courseRun.DurationUnit,
                DurationValue = courseRun.DurationValue,
                FlexibleStartDate = courseRun.FlexibleStartDate,
                StartDate = !courseRun.FlexibleStartDate ? courseRun.StartDate : null,
                StudyMode = courseRun.StudyMode.HasValue ? courseRun.StudyMode.Value : 0,
                National = courseRun.National,
                Course = new CourseViewModel
                {
                    AwardOrgCode = qualification.Record.AwardOrgCode,
                    CourseDescription = HtmlEncode(course.CourseDescription),
                    CourseId = course.CourseId,
                    EntryRequirements = HtmlEncode(course.EntryRequirements),
                    HowYoullBeAssessed = HtmlEncode(course.HowYoullBeAssessed),
                    HowYoullLearn = HtmlEncode(course.HowYoullLearn),
                    LearnAimRef = course.LearnAimRef,
                    QualificationLevel = qualification.Record.NotionalNVQLevelv2,
                    WhatYoullLearn = HtmlEncode(course.WhatYoullLearn),
                    WhatYoullNeed = HtmlEncode(course.WhatYoullNeed),
                    WhereNext = HtmlEncode(course.WhereNext)
                },
                Venue = venue != null
                    ? new VenueViewModel
                    {
                        AddressLine1 = HtmlEncode(venue.AddressLine1),
                        AddressLine2 = HtmlEncode(venue.AddressLine2),
                        County = HtmlEncode(venue.County),
                        Email = venue.Email,
                        Postcode = venue.Postcode,
                        Telephone = venue.Telephone,
                        Town = HtmlEncode(venue.Town),
                        VenueName = HtmlEncode(venue.VenueName),
                        Website = ViewModelFormatting.EnsureHttpPrefixed(venue.Website),
                        Latitude = Convert.ToDecimal(venue.Latitude),
                        Longitude = Convert.ToDecimal(venue.Longitude)
                    }
                    : null,
                Provider = new ProviderViewModel
                {
                    ProviderName = sqlProvider.DisplayName,
                    TradingName = sqlProvider.DisplayName,
                    CourseDirectoryName = provider.CourseDirectoryName,
                    Alias = provider.Alias,
                    Ukprn = provider.Ukprn.ToString(),
                    AddressLine1 = HtmlEncode(providerAddressLines.AddressLine1),
                    AddressLine2 = HtmlEncode(providerAddressLines.AddressLine2),
                    Town = HtmlEncode(providerContact?.ContactAddress?.PostTown ?? providerContact?.ContactAddress?.Items?.FirstOrDefault()?.ToString()),
                    Postcode = providerContact?.ContactAddress?.PostCode,
                    County = HtmlEncode(providerContact?.ContactAddress?.County ?? providerContact?.ContactAddress?.Locality),
                    Telephone = providerContact?.ContactTelephone1,
                    Fax = providerContact?.ContactFax,
                    Website = ViewModelFormatting.EnsureHttpPrefixed(providerContact?.ContactWebsiteAddress),
                    Email = providerContact?.ContactEmail,
                    EmployerSatisfaction = sqlProvider?.EmployerSatisfaction,
                    LearnerSatisfaction = sqlProvider?.LearnerSatisfaction,
                },
                Qualification = new QualificationViewModel
                {
                    AwardOrgCode = qualification.Record.AwardOrgCode,
                    AwardOrgName = HtmlEncode(qualification.Record.AwardOrgName),
                    LearnAimRef = qualification.Record.LearnAimRef,
                    LearnAimRefTitle = HtmlEncode(qualification.Record.LearnAimRefTitle),
                    LearnAimRefTypeDesc = HtmlEncode(qualification.Record.LearnAimRefTypeDesc),
                    QualificationLevel = qualification.Record.NotionalNVQLevelv2,
                    SectorSubjectAreaTier1Desc = HtmlEncode(qualification.Record.SectorSubjectAreaTier1Desc),
                    SectorSubjectAreaTier2Desc = HtmlEncode(qualification.Record.SectorSubjectAreaTier2Desc)
                },
                AlternativeCourseRuns = alternativeCourseRuns.Select(c => new AlternativeCourseRunViewModel
                {
                    CourseRunId = c.CourseRun.CourseRunId,
                    AttendancePattern = c.CourseRun.DeliveryMode == CourseDeliveryMode.ClassroomBased ? (CourseAttendancePattern?)c.CourseRun.AttendancePattern : null,
                    Cost = c.CourseRun.Cost,
                    CostDescription = HtmlEncode(c.CourseRun.CostDescription),
                    CourseName = HtmlEncode(c.CourseRun.CourseName),
                    CourseURL = ViewModelFormatting.EnsureHttpPrefixed(c.CourseRun.CourseWebsite),
                    CreatedDate = c.CourseRun.CreatedOn,
                    DeliveryMode = c.CourseRun.DeliveryMode,
                    DurationUnit = c.CourseRun.DurationUnit,
                    DurationValue = c.CourseRun.DurationValue,
                    FlexibleStartDate = c.CourseRun.FlexibleStartDate,
                    StartDate = !c.CourseRun.FlexibleStartDate ? c.CourseRun.StartDate : null,
                    StudyMode = c.CourseRun.StudyMode.HasValue ? c.CourseRun.StudyMode.Value : 0,
                    Venue = c.Venue != null
                        ? new VenueViewModel
                        {
                            AddressLine1 = HtmlEncode(c.Venue.AddressLine1),
                            AddressLine2 = HtmlEncode(c.Venue.AddressLine2),
                            County = HtmlEncode(c.Venue.County),
                            Email = c.Venue.Email,
                            Postcode = c.Venue.Postcode,
                            Telephone = c.Venue.Telephone,
                            Town = HtmlEncode(c.Venue.Town),
                            VenueName = HtmlEncode(c.Venue.VenueName),
                            Website = ViewModelFormatting.EnsureHttpPrefixed(c.Venue.Website),
                            Latitude = Convert.ToDecimal(c.Venue.Latitude),
                            Longitude = Convert.ToDecimal(c.Venue.Longitude)
                        }
                        : null
                }).ToArray(),
                SubRegions = regions.SelectMany(
                    r => r.SubRegions.Where(sr => courseRun.SubRegionIds?.Contains(sr.Id) ?? false),
                    (r, sr) => new SubRegionViewModel
                    {
                        SubRegionId = sr.Id,
                        Name = sr.Name,
                        ParentRegion = new RegionViewModel
                        {
                            Name = r.Name,
                            RegionId = r.Id
                        }
                    }).ToArray()
            };

            static string HtmlEncode(string value) => System.Net.WebUtility.HtmlEncode(value);

            static (string AddressLine1, string AddressLine2) NormalizeAddress(ProviderContactAddress address)
            {
                if (address == null)
                {
                    return (null, null);
                }

                var parts = new[]
                {
                    address?.SAON?.Description,
                    address?.PAON?.Description,
                    address?.StreetDescription
                }.Where(part => !string.IsNullOrWhiteSpace(part)).ToArray();

                // Join all parts except the last into a single line and make that line 1
                // then the final part is line 2.

                var line1 = string.Join(" ", parts.SkipLast(1));
                var line2 = parts.Skip(1).LastOrDefault();

                return (line1, line2);
            }
        }
    }
}
