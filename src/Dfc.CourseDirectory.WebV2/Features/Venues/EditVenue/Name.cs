using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.VenueValidation;
using FluentValidation;
using FormFlow;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.Venues.EditVenue.Name
{
    public class Query : IRequest<Command>
    {
        public Guid VenueId { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<Command>, Success>>
    {
        public Guid VenueId { get; set; }
        public string Name { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, Command>,
        IRequestHandler<Command, OneOf<ModelWithErrors<Command>, Success>>
    {
        private readonly JourneyInstance<EditVenueJourneyModel> _journeyInstance;
        private readonly IProviderOwnershipCache _providerOwnershipCache;
        private readonly IProviderInfoCache _providerInfoCache;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public Handler(
            JourneyInstance<EditVenueJourneyModel> journeyInstance,
            IProviderOwnershipCache providerOwnershipCache,
            IProviderInfoCache providerInfoCache,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _journeyInstance = journeyInstance;
            _providerOwnershipCache = providerOwnershipCache;
            _providerInfoCache = providerInfoCache;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        public Task<Command> Handle(Query request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Command()
            {
                VenueId = request.VenueId,
                Name = _journeyInstance.State.Name
            });
        }

        public async Task<OneOf<ModelWithErrors<Command>, Success>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var providerId = await _providerOwnershipCache.GetProviderForVenue(request.VenueId);
            var providerInfo = await _providerInfoCache.GetProviderInfo(providerId.Value);

            var validator = new CommandValidator(providerInfo.Ukprn, request.VenueId, _cosmosDbQueryDispatcher);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<Command>(request, validationResult);
            }

            _journeyInstance.UpdateState(state => state.Name = request.Name);

            return new Success();
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(
                int providerUkprn,
                Guid venueId,
                ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
            {
                RuleFor(c => c.Name)
                    .VenueName(providerUkprn, venueId, cosmosDbQueryDispatcher);
            }
        }
    }
}
