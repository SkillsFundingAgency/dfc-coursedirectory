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
                Cost = 0,
                DeliveryMode = CourseDeliveryMode.ClassroomBased,
                DurationUnit = CourseDurationUnit.Years,
                DurationValue = 2,
                EntryRequirements = tLevel.EntryRequirements,
                HowYoullBeAssessed = tLevel.HowYoullBeAssessed,
                HowYoullLearn = tLevel.HowYoullLearn,
                Locations = tLevel.Locations
                    .Select(l => venues[l.VenueId])
                    .Select(v => new TLevelLocationViewModel()
                    {
                        AddressLine1 = v.AddressLine1,
                        AddressLine2 = v.AddressLine2,
                        County = v.County,
                        Email = v.Email,
                        Latitude = Convert.ToDecimal(v.Latitude),
                        Longitude = Convert.ToDecimal(v.Longitude),
                        Postcode = v.Postcode,
                        Telephone = v.Telephone,
                        Town = v.Town,
                        VenueName = v.VenueName,
                        Website = EnsureHttpPrefixed(v.Website)
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
                    QualificationLevel = "3",
                    TLevelName = tLevel.TLevelDefinition.Name
                },
                StartDate = tLevel.StartDate,
                StudyMode = CourseStudyMode.FullTime,
                TLevelId = tLevel.TLevelId,
                Website = tLevel.Website,
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
