using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
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
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var results = await _sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider { ProviderId = request.ProviderId });

            return new ViewModel
            {
                Venues = results.Select(r => new VenueViewModel
                {
                    VenueId = r.VenueId,
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
