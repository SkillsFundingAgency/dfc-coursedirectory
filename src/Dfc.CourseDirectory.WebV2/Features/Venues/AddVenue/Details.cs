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
        public string Telephone { get; set; }
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
        private readonly IProviderInfoCache _providerInfoCache;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly JourneyInstanceProvider _journeyInstanceProvider;

        public Handler(
            IProviderInfoCache providerInfoCache,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            JourneyInstanceProvider journeyInstanceProvider)
        {
            _providerInfoCache = providerInfoCache;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _journeyInstanceProvider = journeyInstanceProvider;
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

            var journeyInstance = _journeyInstanceProvider.GetInstance<AddVenueJourneyModel>();

            journeyInstance.UpdateState(state =>
            {
                state.Name = request.Name;
                state.Email = request.Email;
                state.Telephone = request.Telephone;
                state.Website = request.Website;
                state.ValidStages |= AddVenueCompletedStages.Details;
            });

            return new Success();
        }

        private ViewModel CreateViewModel()
        {
            var journeyInstance = _journeyInstanceProvider.GetInstance<AddVenueJourneyModel>();

            var addressParts = new[]
            {
                journeyInstance.State.AddressLine1,
                journeyInstance.State.AddressLine2,
                journeyInstance.State.Town,
                journeyInstance.State.County,
                journeyInstance.State.Postcode
            }.Where(part => !string.IsNullOrWhiteSpace(part)).ToArray();

            return new ViewModel()
            {
                AddressParts = addressParts,
                Email = journeyInstance.State.Email,
                Name = journeyInstance.State.Name,
                Telephone = journeyInstance.State.Telephone,
                Website = journeyInstance.State.Website
            };
        }

        private void ThrowIfFlowStateNotValid()
        {
            var journeyInstance = _journeyInstanceProvider.GetInstance<AddVenueJourneyModel>();
            journeyInstance.ThrowIfCompleted();

            if (!journeyInstance.State.ValidStages.HasFlag(AddVenueCompletedStages.Address))
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
                RuleFor(c => c.Telephone).PhoneNumber();
                RuleFor(c => c.Website).Website();
            }
        }
    }
}
