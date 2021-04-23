using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.FindACourseApi.Features.TLevels.TLevelDetail
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

            var feChoice = await _sqlQueryDispatcher.ExecuteQuery(new Core.DataStore.Sql.Queries.GetFeChoiceForProvider { ProviderUkprn = provider.Ukprn });

            var providerContact = provider.ProviderContact
                .SingleOrDefault(c => c.ContactType == "P");

            return new TLevelDetailViewModel()
            {
                TLevelId = tLevel.TLevelId,
                TLevelDefinitionId = tLevel.TLevelDefinition.TLevelDefinitionId,
                Qualification = new QualificationViewModel()
                {
                    FrameworkCode = tLevel.TLevelDefinition.FrameworkCode,
                    ProgType = tLevel.TLevelDefinition.ProgType,
                    QualificationLevel = tLevel.TLevelDefinition.QualificationLevel.ToString(),
                    TLevelName = tLevel.TLevelDefinition.Name
                },
                Provider = new ProviderViewModel()
                {
                    ProviderName = sqlProvider.DisplayName,
                    Ukprn = provider.UnitedKingdomProviderReferenceNumber,
                    AddressLine1 = ViewModelFormatting.ConcatAddressLines(providerContact?.ContactAddress?.SAON?.Description, providerContact?.ContactAddress?.PAON?.Description, providerContact?.ContactAddress?.StreetDescription),
                    AddressLine2 = providerContact?.ContactAddress?.Locality,
                    Town = providerContact?.ContactAddress?.PostTown ?? providerContact?.ContactAddress?.Items?.ElementAtOrDefault(0),
                    Postcode = providerContact?.ContactAddress?.PostCode,
                    County = providerContact?.ContactAddress?.County ?? providerContact?.ContactAddress?.Items?.ElementAtOrDefault(1),
                    Email = providerContact?.ContactEmail,
                    Telephone = providerContact?.ContactTelephone1,
                    Fax = providerContact?.ContactFax,
                    Website = ViewModelFormatting.EnsureHttpPrefixed(providerContact?.ContactWebsiteAddress),
                    LearnerSatisfaction = feChoice?.LearnerSatisfaction,
                    EmployerSatisfaction = feChoice?.EmployerSatisfaction
                },
                WhoFor = tLevel.WhoFor,
                EntryRequirements = tLevel.EntryRequirements,
                WhatYoullLearn = tLevel.WhatYoullLearn,
                HowYoullLearn = tLevel.HowYoullLearn,
                HowYoullBeAssessed = tLevel.HowYoullBeAssessed,
                WhatYouCanDoNext = tLevel.WhatYouCanDoNext,
                Website = ViewModelFormatting.EnsureHttpPrefixed(tLevel.Website),
                StartDate = tLevel.StartDate,
                Locations = tLevel.Locations
                    .Select(l => (Venue: venues[l.VenueId], Location: l))
                    .Select(t => new TLevelLocationViewModel()
                    {
                        TLevelLocationId = t.Location.TLevelLocationId,
                        VenueName = t.Venue.VenueName,
                        AddressLine1 = t.Venue.AddressLine1,
                        AddressLine2 = t.Venue.AddressLine2,
                        Town = t.Venue.Town,
                        County = t.Venue.County,
                        Postcode = t.Venue.Postcode,
                        Telephone = t.Venue.Telephone,
                        Email = t.Venue.Email,
                        Website = ViewModelFormatting.EnsureHttpPrefixed(t.Venue.Website),
                        Latitude = Convert.ToDecimal(t.Venue.Latitude),
                        Longitude = Convert.ToDecimal(t.Venue.Longitude)
                    })
                    .ToArray()
            };
        }
    }
}
