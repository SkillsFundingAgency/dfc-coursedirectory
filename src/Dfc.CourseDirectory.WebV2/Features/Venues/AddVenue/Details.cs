using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
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
        public string ProviderVenueRef { get; set; }
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
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly JourneyInstanceProvider _journeyInstanceProvider;

        public Handler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            JourneyInstanceProvider journeyInstanceProvider)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
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

            var validator = new CommandValidator(request.ProviderId, _sqlQueryDispatcher);
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
                state.ProviderVenueRef = request.ProviderVenueRef;
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
                ProviderVenueRef = journeyInstance.State.ProviderVenueRef,
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
                Guid providerId,
                ISqlQueryDispatcher sqlQueryDispatcher)
            {
                RuleFor(c => c.ProviderVenueRef)
                    .ProviderVenueRef(providerId, venueId: null, sqlQueryDispatcher);
                RuleFor(c => c.Name)
                    .VenueName(providerId, venueId: null, sqlQueryDispatcher);
                RuleFor(c => c.Email).Email();
                RuleFor(c => c.Telephone).PhoneNumber();
                RuleFor(c => c.Website).Website();
            }
        }
    }
}
