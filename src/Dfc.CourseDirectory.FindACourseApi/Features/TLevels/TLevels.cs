using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using MediatR;

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

        public Handler(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var tLevels = await _sqlQueryDispatcher.ExecuteQuery(new GetTLevels());

            var providerIds = tLevels.Select(t => t.ProviderId).Distinct();


            var getSqlProviders = _sqlQueryDispatcher.ExecuteQuery(
                new GetProvidersByIds { ProviderIds = providerIds });

            var getVenues = _sqlQueryDispatcher.ExecuteQuery(
                new GetVenuesByIds() { VenueIds = tLevels.SelectMany(t => t.Locations.Select(l => l.VenueId)).Distinct() });

            await Task.WhenAll(getSqlProviders, getVenues);

            var sqlProviders = await getSqlProviders;
            var venues = await getVenues;

            
            return new ViewModel
            {
                TLevels = tLevels.Select(t =>
                {
                    var sqlProvider = sqlProviders[t.ProviderId];
                    var getProviderContact = _sqlQueryDispatcher.ExecuteQuery(new GetProviderContactById { ProviderId = sqlProvider.ProviderId });

                    var providerContact = getProviderContact.Result;

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
                            Ukprn = sqlProvider.Ukprn.ToString(),
                            AddressLine1 = ViewModelFormatting.ConcatAddressLines(providerContact?.AddressSaonDescription, providerContact?.AddressPaonDescription, providerContact?.AddressStreetDescription),
                            AddressLine2 = providerContact?.AddressLocality,
                            Town = providerContact?.AddressPostTown,
                            Postcode = providerContact?.AddressPostcode,
                            County = providerContact?.AddressCounty,
                            Email = providerContact?.Email,
                            Telephone = providerContact?.Telephone1,
                            Fax = providerContact?.Fax,
                            Website = ViewModelFormatting.EnsureHttpPrefixed(providerContact?.WebsiteAddress),
                            LearnerSatisfaction = sqlProvider?.LearnerSatisfaction,
                            EmployerSatisfaction = sqlProvider?.EmployerSatisfaction
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
                                Telephone = String.IsNullOrEmpty(venue.Telephone) ? providerContact?.Telephone1 : venue?.Telephone,
                                Email = String.IsNullOrEmpty(venue.Email) ? providerContact?.Email : venue?.Email,
                                Website = ViewModelFormatting.EnsureHttpPrefixed(venue?.Website) ?? ViewModelFormatting.EnsureHttpPrefixed(providerContact?.WebsiteAddress),
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
