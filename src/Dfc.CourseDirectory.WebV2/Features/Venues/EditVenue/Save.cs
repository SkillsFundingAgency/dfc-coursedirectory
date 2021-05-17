using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
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
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;

        public Handler(
            JourneyInstance<EditVenueJourneyModel> journeyInstance,
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICurrentUserProvider currentUserProvider,
            IClock clock)
        {
            _journeyInstance = journeyInstance;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
        }

        public async Task<Success> Handle(Command request, CancellationToken cancellationToken)
        {
            await _sqlQueryDispatcher.ExecuteQuery(new UpdateVenue()
            {
                VenueId = request.VenueId,
                Name = _journeyInstance.State.Name,
                Email = _journeyInstance.State.Email,
                Telephone = _journeyInstance.State.PhoneNumber,
                Website = _journeyInstance.State.Website,
                AddressLine1 = _journeyInstance.State.AddressLine1,
                AddressLine2 = _journeyInstance.State.AddressLine2,
                Town = _journeyInstance.State.Town,
                County = _journeyInstance.State.County,
                Postcode = _journeyInstance.State.Postcode,
                Latitude = _journeyInstance.State.Latitude,
                Longitude = _journeyInstance.State.Longitude,
                UpdatedOn = _clock.UtcNow,
                UpdatedBy = _currentUserProvider.GetCurrentUser()
            });

            return new Success();
        }
    }
}
