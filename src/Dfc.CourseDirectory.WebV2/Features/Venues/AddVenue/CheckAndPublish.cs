using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Security;
using FormFlow;
using MediatR;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.Venues.AddVenue.CheckAndPublish
{
    public class Query : IRequest<ViewModel>
    {
    }

    public class ViewModel
    {
        public string Name { get; set; }
        public IReadOnlyCollection<string> AddressParts { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Website { get; set; }
        public bool NewAddressIsOutsideOfEngland { get; set; }
    }

    public class Command : IRequest<Success>
    {
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, Success>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly JourneyInstanceProvider _journeyInstanceProvider;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;
        private readonly IProviderContextProvider _providerContextProvider;

        public Handler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            JourneyInstanceProvider journeyInstanceProvider,
            ICurrentUserProvider currentUserProvider,
            IClock clock,
            IProviderContextProvider providerContextProvider)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _journeyInstanceProvider = journeyInstanceProvider;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
            _providerContextProvider = providerContextProvider;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            ThrowIfFlowStateNotValid();

            var journeyInstance = _journeyInstanceProvider.GetInstance<AddVenueJourneyModel>();

            var addressParts = new[]
            {
                journeyInstance.State.AddressLine1,
                journeyInstance.State.AddressLine2,
                journeyInstance.State.Town,
                journeyInstance.State.County,
                journeyInstance.State.Postcode
            }.Where(part => !string.IsNullOrWhiteSpace(part)).ToArray();

            var vm = new ViewModel()
            {
                AddressParts = addressParts,
                Email = journeyInstance.State.Email,
                Name = journeyInstance.State.Name,
                Telephone = journeyInstance.State.Telephone,
                Website = journeyInstance.State.Website,
                NewAddressIsOutsideOfEngland = journeyInstance.State.AddressIsOutsideOfEngland
            };

            return Task.FromResult(vm);
        }

        public async Task<Success> Handle(Command request, CancellationToken cancellationToken)
        {
            ThrowIfFlowStateNotValid();

            var journeyInstance = _journeyInstanceProvider.GetInstance<AddVenueJourneyModel>();

            var venueId = Guid.NewGuid();
            var providerUkprn = _providerContextProvider.GetProviderContext().ProviderInfo.Ukprn;

            await _sqlQueryDispatcher.ExecuteQuery(new CreateVenue()
            {
                VenueId = venueId,
                ProviderUkprn = providerUkprn,
                Name = journeyInstance.State.Name,
                Email = journeyInstance.State.Email,
                Telephone = journeyInstance.State.Telephone,
                Website = journeyInstance.State.Website,
                AddressLine1 = journeyInstance.State.AddressLine1,
                AddressLine2 = journeyInstance.State.AddressLine2,
                Town = journeyInstance.State.Town,
                County = journeyInstance.State.County,
                Postcode = journeyInstance.State.Postcode,
                Latitude = Convert.ToDecimal(journeyInstance.State.Latitude),
                Longitude = Convert.ToDecimal(journeyInstance.State.Longitude),
                CreatedBy = _currentUserProvider.GetCurrentUser(),
                CreatedOn = _clock.UtcNow
            });

            journeyInstance.UpdateState(state => state.VenueId = venueId);

            // Complete JourneyInstance so state can no longer be changed
            journeyInstance.Complete();

            return new Success();
        }

        private void ThrowIfFlowStateNotValid()
        {
            var journeyInstance = _journeyInstanceProvider.GetInstance<AddVenueJourneyModel>();
            journeyInstance.ThrowIfCompleted();

            if (!journeyInstance.State.ValidStages.HasFlags(AddVenueCompletedStages.All))
            {
                throw new InvalidStateException();
            }
        }
    }
}
