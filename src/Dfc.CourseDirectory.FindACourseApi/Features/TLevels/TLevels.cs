using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using MediatR;
using CosmosQueries = Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.FindACourseApi.Features.TLevels
{
    public class Query : IRequest<ViewModel>
    {
    }

    public class ViewModel
    {
        public IReadOnlyCollection<TLevelDetailViewModel> TLevels { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public Handler(ISqlQueryDispatcher sqlQueryDispatcher, ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var tLevels = await _sqlQueryDispatcher.ExecuteQuery(new GetTLevels());

            var providerIds = tLevels.Select(t => t.ProviderId).Distinct();

            var getProviders = _cosmosDbQueryDispatcher.ExecuteQuery(
                new CosmosQueries.GetProvidersByIds { ProviderIds = providerIds });

            var getSqlProviders = _sqlQueryDispatcher.ExecuteQuery(
                new GetProvidersByIds { ProviderIds = providerIds });

            var getVenues = _sqlQueryDispatcher.ExecuteQuery(
                new GetVenuesByIds() { VenueIds = tLevels.SelectMany(t => t.Locations.Select(l => l.VenueId)).Distinct() });

            await Task.WhenAll(getProviders, getSqlProviders, getVenues);

            var providers = await getProviders;
            var sqlProviders = await getSqlProviders;
            var venues = await getVenues;

            var feChoices = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new CosmosQueries.GetFeChoicesByProviderUkprns { ProviderUkprns = providers.Values.Select(p => p.Ukprn) });

            return new ViewModel
            {
                TLevels = tLevels.Select(t =>
                {
                    var provider = providers[t.ProviderId];
                    var sqlProvider = sqlProviders[t.ProviderId];
                    var feChoice = feChoices.GetValueOrDefault(provider.Ukprn);
                    var providerContact = provider.ProviderContact
                        .SingleOrDefault(c => c.ContactType == "P");

                    return new TLevelDetailViewModel
                    {
                        TLevelId = t.TLevelId,
                        TLevelDefinitionId = t.TLevelDefinition.TLevelDefinitionId,
                        Qualification = new QualificationViewModel
                        {
                            TLevelName = t.TLevelDefinition.Name,
                            FrameworkCode = t.TLevelDefinition.FrameworkCode,
                            ProgType = t.TLevelDefinition.ProgType,
                            QualificationLevel = t.TLevelDefinition.QualificationLevel.ToString(),
                        },
                        Provider = new ProviderViewModel
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
                        WhoFor = t.WhoFor,
                        EntryRequirements = t.EntryRequirements,
                        WhatYoullLearn = t.WhatYoullLearn,
                        HowYoullLearn = t.HowYoullLearn,
                        HowYoullBeAssessed = t.HowYoullBeAssessed,
                        WhatYouCanDoNext = t.WhatYouCanDoNext,
                        Website = ViewModelFormatting.EnsureHttpPrefixed(t.Website),
                        StartDate = t.StartDate,
                        Locations = t.Locations.Select(l =>
                        {
                            var venue = venues[l.VenueId];

                            return new TLevelLocationViewModel
                            {
                                TLevelLocationId = l.TLevelLocationId,
                                VenueName = venue.VenueName,
                                AddressLine1 = venue.AddressLine1,
                                AddressLine2 = venue.AddressLine2,
                                Town = venue.Town,
                                County = venue.County,
                                Postcode = venue.Postcode,
                                Telephone = String.IsNullOrEmpty(venue.Telephone) ? providerContact.ContactTelephone1 : 
                                    venue.Telephone,
                                Email = String.IsNullOrEmpty(venue.Email) ? providerContact.ContactEmail : venue.Email,
                                Website = String.IsNullOrEmpty(ViewModelFormatting.EnsureHttpPrefixed(venue.Website)) ? 
                                    ViewModelFormatting.EnsureHttpPrefixed(providerContact?.ContactWebsiteAddress) : 
                                    ViewModelFormatting.EnsureHttpPrefixed(venue.Website),
                                Latitude = Convert.ToDecimal(venue.Latitude),
                                Longitude = Convert.ToDecimal(venue.Longitude)
                            };
                        }).ToArray(),
                    };
                }).ToArray()
            };
        }
    }
}
