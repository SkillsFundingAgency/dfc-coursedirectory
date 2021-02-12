using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.VenueValidation;
using FluentValidation;
using FormFlow;
using Mapster;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.Venues.AddVenue.Details
{
    public class Query : IRequest<ViewModel>
    {
    }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        public Guid ProviderId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Website { get; set; }
    }

    public class ViewModel : Command
    {
        public IReadOnlyCollection<string> AddressParts { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        private readonly JourneyInstance<AddVenueJourneyModel> _journeyInstance;
        private readonly IProviderInfoCache _providerInfoCache;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public Handler(
            IProviderInfoCache providerInfoCache,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            JourneyInstanceProvider journeyInstanceProvider)
        {
            _journeyInstance = journeyInstanceProvider.GetInstance<AddVenueJourneyModel>();
            _providerInfoCache = providerInfoCache;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            ThrowIfFlowStateNotValid();

            return Task.FromResult(CreateViewModel());
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            ThrowIfFlowStateNotValid();

            var providerInfo = await _providerInfoCache.GetProviderInfo(request.ProviderId);

            var validator = new CommandValidator(providerInfo.Ukprn, _cosmosDbQueryDispatcher);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var vm = CreateViewModel();
                vm.Adapt(request);
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            _journeyInstance.UpdateState(state =>
            {
                state.Name = request.Name;
                state.Email = request.Email;
                state.PhoneNumber = request.PhoneNumber;
                state.Website = request.Website;
                state.ValidStages |= AddVenueCompletedStages.Details;
            });

            return new Success();
        }

        private ViewModel CreateViewModel()
        {
            var state = _journeyInstance.State;

            var addressParts = new[]
            {
                state.AddressLine1,
                state.AddressLine2,
                state.Town,
                state.County,
                state.Postcode
            }.Where(part => !string.IsNullOrWhiteSpace(part)).ToArray();

            return new ViewModel()
            {
                AddressParts = addressParts,
                Email = state.Email,
                Name = state.Name,
                PhoneNumber = state.PhoneNumber,
                Website = state.Website
            };
        }

        private void ThrowIfFlowStateNotValid()
        {
            _journeyInstance.ThrowIfCompleted();

            if (!_journeyInstance.State.ValidStages.HasFlag(AddVenueCompletedStages.Address))
            {
                throw new InvalidStateException();
            }
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(
                int providerUkprn,
                ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
            {
                RuleFor(c => c.Name)
                    .VenueName(providerUkprn, venueId: null, cosmosDbQueryDispatcher);
                RuleFor(c => c.Email).Email();
                RuleFor(c => c.PhoneNumber).PhoneNumber();
                RuleFor(c => c.Website).Website();
            }
        }
    }
}
