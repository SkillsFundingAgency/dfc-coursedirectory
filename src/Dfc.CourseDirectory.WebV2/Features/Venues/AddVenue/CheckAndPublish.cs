using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
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
        public string PhoneNumber { get; set; }
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
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly JourneyInstance<AddVenueJourneyModel> _journeyInstance;

        public Handler(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            JourneyInstanceProvider journeyInstanceProvider,
            ICurrentUserProvider currentUserProvider,
            IClock clock,
            IProviderContextProvider providerContextProvider)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
            _providerContextProvider = providerContextProvider;
            _journeyInstance = journeyInstanceProvider.GetInstance<AddVenueJourneyModel>();
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            ThrowIfFlowStateNotValid();

            var state = _journeyInstance.State;

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
                Name = state.Name,
                PhoneNumber = state.PhoneNumber,
                Website = state.Website,
                NewAddressIsOutsideOfEngland = state.AddressIsOutsideOfEngland
            };

            return Task.FromResult(vm);
        }

        public async Task<Success> Handle(Command request, CancellationToken cancellationToken)
        {
            ThrowIfFlowStateNotValid();

            var venueId = Guid.NewGuid();
            var providerUkprn = _providerContextProvider.GetProviderContext().ProviderInfo.Ukprn;

            await _cosmosDbQueryDispatcher.ExecuteQuery(new CreateVenue()
            {
                VenueId = venueId,
                ProviderUkprn = providerUkprn,
                Name = _journeyInstance.State.Name,
                Email = _journeyInstance.State.Email,
                PhoneNumber = _journeyInstance.State.PhoneNumber,
                Website = _journeyInstance.State.Website,
                AddressLine1 = _journeyInstance.State.AddressLine1,
                AddressLine2 = _journeyInstance.State.AddressLine2,
                Town = _journeyInstance.State.Town,
                County = _journeyInstance.State.County,
                Postcode = _journeyInstance.State.Postcode,
                Latitude = _journeyInstance.State.Latitude,
                Longitude = _journeyInstance.State.Longitude,
                CreatedBy = _currentUserProvider.GetCurrentUser(),
                CreatedDate = _clock.UtcNow
            });

            _journeyInstance.UpdateState(state => state.VenueId = venueId);

            // Complete JourneyInstance so state can no longer be changed
            _journeyInstance.Complete();

            return new Success();
        }

        private void ThrowIfFlowStateNotValid()
        {
            _journeyInstance.ThrowIfCompleted();

            if (!_journeyInstance.State.ValidStages.HasFlags(AddVenueCompletedStages.All))
            {
                throw new InvalidStateException();
            }
        }
    }
}
