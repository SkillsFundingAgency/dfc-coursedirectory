using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
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
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ISearchClient<Core.Search.Models.Lars> _larsSearchClient;
        private readonly IRegionCache _regionCache;

        public Handler(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ISqlQueryDispatcher sqlQueryDispatcher,
            ISearchClient<Core.Search.Models.Lars> larsSearchClient,
            IRegionCache regionCache)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher ?? throw new ArgumentNullException(nameof(cosmosDbQueryDispatcher));
            _sqlQueryDispatcher = sqlQueryDispatcher ?? throw new ArgumentNullException(nameof(sqlQueryDispatcher));
            _larsSearchClient = larsSearchClient ?? throw new ArgumentNullException(nameof(larsSearchClient));
            _regionCache = regionCache ?? throw new ArgumentNullException(nameof(regionCache));
        }

        public async Task<OneOf<NotFound, CourseRunDetailViewModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            var course = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetCourseById { CourseId = request.CourseId });

            if (course == null)
            {
                return new NotFound();
            }

            var courseRun = course.CourseRuns.SingleOrDefault(c => c.Id == request.CourseRunId && c.RecordStatus == CourseStatus.Live);

            if (courseRun == null)
            {
                return new NotFound();
            }

            var getProvider = _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderByUkprn { Ukprn = course.ProviderUKPRN });
            var getQualification = _larsSearchClient.Search(new LarsLearnAimRefSearchQuery { LearnAimRef = course.LearnAimRef });
            var getFeChoice = _cosmosDbQueryDispatcher.ExecuteQuery(new GetFeChoiceForProvider { ProviderUkprn = course.ProviderUKPRN });

            await Task.WhenAll(getProvider, getQualification, getFeChoice);

            var provider = getProvider.Result;
            var qualification = getQualification.Result.Items.SingleOrDefault();
            var feChoice = getFeChoice.Result;

            var getSqlProvider = _sqlQueryDispatcher.ExecuteQuery(new Core.DataStore.Sql.Queries.GetProviderById { ProviderId = provider.Id });
            var getProviderVenues = _sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = provider.Id });

            await Task.WhenAll(getSqlProvider, getProviderVenues);

            var sqlProvider = getSqlProvider.Result;
            var venues = getProviderVenues.Result;

            var venue = courseRun.VenueId.HasValue
                ? venues.Single(v => v.VenueId == courseRun.VenueId)
                : null;

            var providerContact = provider.ProviderContact.SingleOrDefault(c => c.ContactType == "P");
            var providerAddressLines = NormalizeAddress(providerContact?.ContactAddress);

            var alternativeCourseRuns = course.CourseRuns
                .Where(r => r.Id != request.CourseRunId && r.RecordStatus == CourseStatus.Live)
                .Select(r => new { CourseRun = r, Venue = venues.SingleOrDefault(v => v.VenueId == r.VenueId) });

            var regions = await _regionCache.GetAllRegions();

            return new CourseRunDetailViewModel
            {
                CourseRunId = courseRun.Id,
                OfferingType = Core.Search.Models.FindACourseOfferingType.Course,
                AttendancePattern = courseRun.DeliveryMode == CourseDeliveryMode.ClassroomBased ? (CourseAttendancePattern?)courseRun.AttendancePattern : null,
                Cost = courseRun.Cost,
                CostDescription = courseRun.CostDescription,
                CourseName = courseRun.CourseName,
                CourseURL = ViewModelFormatting.EnsureHttpPrefixed(courseRun.CourseURL),
                CreatedDate = courseRun.CreatedDate,
                DeliveryMode = courseRun.DeliveryMode,
                DurationUnit = courseRun.DurationUnit,
                DurationValue = courseRun.DurationValue,
                FlexibleStartDate = courseRun.FlexibleStartDate,
                StartDate = !courseRun.FlexibleStartDate ? courseRun.StartDate : null,
                StudyMode = courseRun.StudyMode,
                National = courseRun.National,
                Course = new CourseViewModel
                {
                    AdvancedLearnerLoan = course.AdvancedLearnerLoan,
                    AwardOrgCode = course.AwardOrgCode,
                    CourseDescription = course.CourseDescription,
                    CourseId = course.Id,
                    EntryRequirements = course.EntryRequirements,
                    HowYoullBeAssessed = course.HowYoullBeAssessed,
                    HowYoullLearn = course.HowYoullLearn,
                    LearnAimRef = course.LearnAimRef,
                    QualificationLevel = course.NotionalNVQLevelv2,
                    WhatYoullLearn = course.WhatYoullLearn,
                    WhatYoullNeed = course.WhatYoullNeed,
                    WhereNext = course.WhereNext
                },
                Venue = venue != null
                    ? new VenueViewModel
                    {
                        AddressLine1 = venue.AddressLine1,
                        AddressLine2 = venue.AddressLine2,
                        County = venue.County,
                        Email = venue.Email,
                        Postcode = venue.Postcode,
                        Telephone = venue.Telephone,
                        Town = venue.Town,
                        VenueName = venue.VenueName,
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
                    Ukprn = provider.UnitedKingdomProviderReferenceNumber,
                    AddressLine1 = providerAddressLines.AddressLine1,
                    AddressLine2 = providerAddressLines.AddressLine2,
                    Town = providerContact?.ContactAddress?.PostTown ?? providerContact?.ContactAddress?.Items?.FirstOrDefault()?.ToString(),
                    Postcode = providerContact?.ContactAddress?.PostCode,
                    County = providerContact?.ContactAddress?.County ?? providerContact?.ContactAddress?.Locality,
                    Telephone = providerContact?.ContactTelephone1,
                    Fax = providerContact?.ContactFax,
                    Website = ViewModelFormatting.EnsureHttpPrefixed(providerContact?.ContactWebsiteAddress),
                    Email = providerContact?.ContactEmail,
                    EmployerSatisfaction = feChoice?.EmployerSatisfaction,
                    LearnerSatisfaction = feChoice?.LearnerSatisfaction,
                },
                Qualification = new QualificationViewModel
                {
                    AwardOrgCode = qualification.Record.AwardOrgCode,
                    AwardOrgName = qualification.Record.AwardOrgName,
                    LearnAimRef = qualification.Record.LearnAimRef,
                    LearnAimRefTitle = qualification.Record.LearnAimRefTitle,
                    LearnAimRefTypeDesc = qualification.Record.LearnAimRefTypeDesc,
                    QualificationLevel = qualification.Record.NotionalNVQLevelv2,
                    SectorSubjectAreaTier1Desc = qualification.Record.SectorSubjectAreaTier1Desc,
                    SectorSubjectAreaTier2Desc = qualification.Record.SectorSubjectAreaTier2Desc
                },
                AlternativeCourseRuns = alternativeCourseRuns.Select(c => new AlternativeCourseRunViewModel
                {
                    CourseRunId = c.CourseRun.Id,
                    AttendancePattern = c.CourseRun.DeliveryMode == CourseDeliveryMode.ClassroomBased ? (CourseAttendancePattern?)c.CourseRun.AttendancePattern : null,
                    Cost = c.CourseRun.Cost,
                    CostDescription = c.CourseRun.CostDescription,
                    CourseName = c.CourseRun.CourseName,
                    CourseURL = ViewModelFormatting.EnsureHttpPrefixed(c.CourseRun.CourseURL),
                    CreatedDate = c.CourseRun.CreatedDate,
                    DeliveryMode = c.CourseRun.DeliveryMode,
                    DurationUnit = c.CourseRun.DurationUnit,
                    DurationValue = c.CourseRun.DurationValue,
                    FlexibleStartDate = c.CourseRun.FlexibleStartDate,
                    StartDate = !c.CourseRun.FlexibleStartDate ? c.CourseRun.StartDate : null,
                    StudyMode = c.CourseRun.StudyMode,
                    Venue = c.Venue != null
                        ? new VenueViewModel
                        {
                            AddressLine1 = c.Venue.AddressLine1,
                            AddressLine2 = c.Venue.AddressLine2,
                            County = c.Venue.County,
                            Email = c.Venue.Email,
                            Postcode = c.Venue.Postcode,
                            Telephone = c.Venue.Telephone,
                            Town = c.Venue.Town,
                            VenueName = c.Venue.VenueName,
                            Website = ViewModelFormatting.EnsureHttpPrefixed(c.Venue.Website),
                            Latitude = Convert.ToDecimal(c.Venue.Latitude),
                            Longitude = Convert.ToDecimal(c.Venue.Longitude)
                        }
                        : null
                }).ToArray(),
                SubRegions = regions.SelectMany(
                    r => r.SubRegions.Where(sr => courseRun.Regions?.Contains(sr.Id) ?? false),
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
