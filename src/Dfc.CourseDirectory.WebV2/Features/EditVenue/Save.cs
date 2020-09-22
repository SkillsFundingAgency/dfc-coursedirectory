using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.Behaviors;
using FormFlow;
using MediatR;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.EditVenue.Save
{
    public class Command : IRequest<Success>
    {
        public Guid VenueId { get; set; }
    }

    public class Handler :
        IRequireUserCanAccessVenue<Command>,
        IRequestHandler<Command, Success>
    {
        private readonly FormFlowInstance<EditVenueFlowModel> _formFlowInstance;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public Handler(
            FormFlowInstance<EditVenueFlowModel> formFlowInstance,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _formFlowInstance = formFlowInstance;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        public async Task<Success> Handle(Command request, CancellationToken cancellationToken)
        {
            await _cosmosDbQueryDispatcher.ExecuteQuery(new UpdateVenue()
            {
                VenueId = request.VenueId,
                Name = _formFlowInstance.State.Name,
                Email = _formFlowInstance.State.Email,
                PhoneNumber = _formFlowInstance.State.PhoneNumber,
                Website = _formFlowInstance.State.Website,
                AddressLine1 = _formFlowInstance.State.AddressLine1,
                AddressLine2 = _formFlowInstance.State.AddressLine2,
                Town = _formFlowInstance.State.Town,
                County = _formFlowInstance.State.County,
                Postcode = _formFlowInstance.State.Postcode,
                Latitude = _formFlowInstance.State.Latitude,
                Longitude = _formFlowInstance.State.Longitude
            });

            return new Success();
        }

        Guid IRequireUserCanAccessVenue<Command>.GetVenueId(Command request) => request.VenueId;
    }
}
