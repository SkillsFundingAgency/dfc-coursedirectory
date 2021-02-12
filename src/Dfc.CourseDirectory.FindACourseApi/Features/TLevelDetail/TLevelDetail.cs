using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.FindACourseApi.Features.TLevelDetail
{
    public class Query : IRequest<OneOf<NotFound, TLevelDetailViewModel>>
    {
        public Guid TLevelId { get; set; }
    }

    public class Handler : IRequestHandler<Query, OneOf<NotFound, TLevelDetailViewModel>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public Handler(ISqlQueryDispatcher sqlQueryDispatcher, ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        public async Task<OneOf<NotFound, TLevelDetailViewModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tLevel = await _sqlQueryDispatcher.ExecuteQuery(new GetTLevel() { TLevelId = request.TLevelId });

            if (tLevel == null)
            {
                return new NotFound();
            }

            var getProvider = _cosmosDbQueryDispatcher.ExecuteQuery(
                new Core.DataStore.CosmosDb.Queries.GetProviderById() { ProviderId = tLevel.ProviderId });

            var getSqlProvider = _sqlQueryDispatcher.ExecuteQuery(
                new Core.DataStore.Sql.Queries.GetProviderById { ProviderId = tLevel.ProviderId });

            var getVenues = _sqlQueryDispatcher.ExecuteQuery(
                new GetVenuesByIds() { VenueIds = tLevel.Locations.Select(l => l.VenueId) });

            await Task.WhenAll(getProvider, getSqlProvider, getVenues);

            var provider = await getProvider;
            var sqlProvider = await getSqlProvider;
            var venues = await getVenues;

            var feChoice = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetFeChoiceForProvider() { ProviderUkprn = provider.Ukprn });

            var providerContact = provider.ProviderContact
                .SingleOrDefault(c => c.ContactType == "P");

            return new TLevelDetailViewModel()
            {
                AttendancePattern = CourseAttendancePattern.Daytime,
                CostDescription = "T Levels are currently only available to 16-19 year olds. Contact us for details of other suitable courses.",
                DeliveryMode = CourseDeliveryMode.ClassroomBased,
                DurationUnit = CourseDurationUnit.Years,
                DurationValue = 2,
                EntryRequirements = tLevel.EntryRequirements,
                HowYoullBeAssessed = tLevel.HowYoullBeAssessed,
                HowYoullLearn = tLevel.HowYoullLearn,
                Locations = tLevel.Locations
                    .Select(l => (Venue: venues[l.VenueId], Location: l))
                    .Select(t => new TLevelLocationViewModel()
                    {
                        TLevelLocationId = t.Location.TLevelLocationId,
                        AddressLine1 = t.Venue.AddressLine1,
                        AddressLine2 = t.Venue.AddressLine2,
                        County = t.Venue.County,
                        Email = t.Venue.Email,
                        Latitude = Convert.ToDecimal(t.Venue.Latitude),
                        Longitude = Convert.ToDecimal(t.Venue.Longitude),
                        Postcode = t.Venue.Postcode,
                        Telephone = t.Venue.Telephone,
                        Town = t.Venue.Town,
                        VenueName = t.Venue.VenueName,
                        Website = EnsureHttpPrefixed(t.Venue.Website)
                    })
                    .ToArray(),
                OfferingType = Core.Search.Models.FindACourseOfferingType.TLevel,
                Provider = new ProviderViewModel()
                {
                    ProviderName = sqlProvider.DisplayName,
                    Ukprn = provider.UnitedKingdomProviderReferenceNumber,
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
                    LearnerSatisfaction = feChoice?.LearnerSatisfaction
                },
                Qualification = new QualificationViewModel()
                {
                    FrameworkCode = tLevel.TLevelDefinition.FrameworkCode,
                    ProgType = tLevel.TLevelDefinition.ProgType,
                    QualificationLevel = tLevel.TLevelDefinition.QualificationLevel.ToString(),
                    TLevelName = tLevel.TLevelDefinition.Name
                },
                StartDate = tLevel.StartDate,
                StudyMode = CourseStudyMode.FullTime,
                TLevelId = tLevel.TLevelId,
                Website = EnsureHttpPrefixed(tLevel.Website),
                WhatYouCanDoNext = tLevel.WhatYouCanDoNext,
                WhatYoullLearn = tLevel.WhatYoullLearn,
                WhoFor = tLevel.WhoFor
            };
        }

        private static string EnsureHttpPrefixed(string url) => !string.IsNullOrEmpty(url)
            ? url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? url
                : $"http://{url}"
            : null;
    }
}
