using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.Security;
using FormFlow;
using MediatR;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.Venues.EditVenue.Save
{
    public class Command : IRequest<Success>
    {
        public Guid VenueId { get; set; }
    }

    public class Handler : IRequestHandler<Command, Success>
    {
        private readonly JourneyInstance<EditVenueJourneyModel> _journeyInstance;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;
        private readonly SqlDataSync _sqlDataSync;

        public Handler(
            JourneyInstance<EditVenueJourneyModel> journeyInstance,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ICurrentUserProvider currentUserProvider,
            IClock clock,
            SqlDataSync sqlDataSync)
        {
            _journeyInstance = journeyInstance;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
            _sqlDataSync = sqlDataSync;
        }

        public async Task<Success> Handle(Command request, CancellationToken cancellationToken)
        {
            var result =  await _cosmosDbQueryDispatcher.ExecuteQuery(new UpdateVenue()
            {
                VenueId = request.VenueId,
                Name = _journeyInstance.State.Name,
                Email = _journeyInstance.State.Email,
                PhoneNumber = _journeyInstance.State.PhoneNumber,
                Website = _journeyInstance.State.Website,
                AddressLine1 = _journeyInstance.State.AddressLine1,
                AddressLine2 = _journeyInstance.State.AddressLine2,
                Town = _journeyInstance.State.Town,
                County = _journeyInstance.State.County,
                Postcode = _journeyInstance.State.Postcode,
                Latitude = Convert.ToDecimal(_journeyInstance.State.Latitude),
                Longitude = Convert.ToDecimal(_journeyInstance.State.Longitude),
                UpdatedDate = _clock.UtcNow,
                UpdatedBy = _currentUserProvider.GetCurrentUser()
            });

            var venue = (Venue)result.Value;

            await _sqlDataSync.SyncVenue(venue);

            return new Success();
        }
    }
}
