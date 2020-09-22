using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors;
using FormFlow;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.EditVenue.Details
{
    public class Query : IRequest<ViewModel>
    {
        public Guid VenueId { get; set; }
    }

    public class ViewModel
    {
        public Guid VenueId { get; set; }
        public string LocationName { get; set; }
        public IReadOnlyCollection<string> AddressParts { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Website { get; set; }
        public bool NewAddressIsOutsideOfEngland { get; set; }
    }

    public class Handler :
        IRequireUserCanAccessVenue<Query>,
        IRequestHandler<Query, ViewModel>
    {
        private readonly FormFlowInstance<EditVenueFlowModel> _formFlowInstance;

        public Handler(FormFlowInstance<EditVenueFlowModel> formFlowInstance)
        {
            _formFlowInstance = formFlowInstance;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var state = _formFlowInstance.State;

            var addressParts = new[]
            {
                state.AddressLine1,
                state.AddressLine2,
                state.Town,
                state.County,
                state.Postcode
            }.Where(part => !string.IsNullOrWhiteSpace(part)).ToArray();

            var vm = new ViewModel()
            {
                AddressParts = addressParts,
                Email = state.Email,
                LocationName = state.Name,
                PhoneNumber = state.PhoneNumber,
                VenueId = request.VenueId,
                Website = state.Website,
                NewAddressIsOutsideOfEngland = state.NewAddressIsOutsideOfEngland
            };

            return Task.FromResult(vm);
        }

        Guid IRequireUserCanAccessVenue<Query>.GetVenueId(Query request) => request.VenueId;
    }
}
