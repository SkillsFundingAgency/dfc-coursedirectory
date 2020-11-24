using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.FindACourseApi.ViewModels;
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
        private readonly ISearchClient<Core.Search.Models.Lars> _larsSearchClient;

        public Handler(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher, ISearchClient<Core.Search.Models.Lars> larsSearchClient)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher ?? throw new ArgumentNullException(nameof(cosmosDbQueryDispatcher));
            _larsSearchClient = larsSearchClient ?? throw new ArgumentNullException(nameof(larsSearchClient));
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
            var getVenues = _cosmosDbQueryDispatcher.ExecuteQuery(new GetVenuesByProvider { ProviderUkprn = course.ProviderUKPRN });
            var getFeChoice = _cosmosDbQueryDispatcher.ExecuteQuery(new GetFeChoiceForProvider { ProviderUkprn = course.ProviderUKPRN });

            await Task.WhenAll(getProvider, getQualification, getVenues, getFeChoice);

            var provider = await getProvider;
            var qualification = (await getQualification).Items.SingleOrDefault();
            var venues = await getVenues;
            var feChoice = await getFeChoice;

            var venue = courseRun.VenueId.HasValue
                ? venues.Single(v => v.Id == courseRun.VenueId)
                : null;

            var providerContact = provider.ProviderContact
                .SingleOrDefault(c => c.ContactType == "P");

            var alternativeCourseRuns = course.CourseRuns
                .Where(r => r.Id != request.CourseRunId && r.RecordStatus == CourseStatus.Live)
                .Select(r => new { CourseRun = r, Venue = venues.SingleOrDefault(v => v.Id == r.VenueId) });

            return new CourseRunDetailViewModel
            {
                CourseRunId = courseRun.Id,
                AttendancePattern = courseRun.DeliveryMode == CourseDeliveryMode.ClassroomBased ? (CourseAttendancePattern?)courseRun.AttendancePattern : null,
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
                        Telephone = venue.PHONE,
                        Town = venue.Town,
                        VenueName = venue.VenueName,
                        Website = EnsureHttpPrefixed(venue.Website),
                        Latitude = venue.Latitude,
                        Longitude = venue.Longitude
                    }
                    : null,
                Provider = new ProviderViewModel
                {
                    ProviderName = provider.ProviderName,
                    TradingName = provider.TradingName,
                    CourseDirectoryName = provider.CourseDirectoryName,
                    Alias = provider.Alias,
                    UKPRN = provider.UnitedKingdomProviderReferenceNumber,
                    AddressLine1 = providerContact?.ContactAddress?.SAON?.Description,
                    AddressLine2 = providerContact?.ContactAddress?.PAON?.Description,
                    Town = providerContact?.ContactAddress?.Items?.FirstOrDefault()?.ToString(),
                    Postcode = providerContact?.ContactAddress?.PostCode,
                    County = providerContact?.ContactAddress?.Locality,
                    Telephone = providerContact?.ContactTelephone1,
                    Fax = providerContact?.ContactFax,
                    Website = EnsureHttpPrefixed(providerContact?.ContactWebsiteAddress),
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
                    CourseURL = c.CourseRun.CourseURL,
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
                            Telephone = c.Venue.PHONE,
                            Town = c.Venue.Town,
                            VenueName = c.Venue.VenueName,
                            Website = EnsureHttpPrefixed(c.Venue.Website),
                            Latitude = c.Venue.Latitude,
                            Longitude = c.Venue.Longitude
                        }
                        : null
                }).ToArray(),
                SubRegions = Region.All.SelectMany(
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
        }

        private static string EnsureHttpPrefixed(string url) => !string.IsNullOrEmpty(url)
            ? url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? url
                : $"http://{url}"
            : null;
    }
}
