using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.Venues.ViewVenues
{
    public class Query : IRequest<ViewModel>
    {
        public Guid ProviderId { get; set; }
    }

    public class ViewModel
    {
        public IReadOnlyCollection<VenueViewModel> Venues { get; set; }
    }

    public class VenueViewModel
    {
        public Guid VenueId { get; set; }
        public string VenueName { get; set; }
        public IReadOnlyCollection<string> AddressParts { get; set; }
        public string PostCode { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IProviderInfoCache _providerInfoCache;

        public Handler(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher, IProviderInfoCache providerInfoCache)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _providerInfoCache = providerInfoCache;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var provider = await _providerInfoCache.GetProviderInfo(request.ProviderId);

            var results = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetVenuesByProvider { ProviderUkprn = provider.Ukprn });

            return new ViewModel
            {
                Venues = results.Select(r => new VenueViewModel
                {
                    VenueId = r.Id,
                    VenueName = r.VenueName,
                    AddressParts = new[]
                    {
                        r.AddressLine1,
                        r.AddressLine2,
                        r.Town,
                        r.County
                    }.Where(part => !string.IsNullOrWhiteSpace(part)).ToArray(),
                    PostCode = r.Postcode
                }).ToArray()
            };
        }
    }
}
