using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.VenueValidation;
using FluentValidation;
using FormFlow;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.Venues.EditVenue.ProviderVenueRef
{
    public class Query : IRequest<Command>
    {
        public Guid VenueId { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<Command>, Success>>
    {
        public Guid VenueId { get; set; }
        public string ProviderVenueRef { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, Command>,
        IRequestHandler<Command, OneOf<ModelWithErrors<Command>, Success>>
    {
        private readonly JourneyInstance<EditVenueJourneyModel> _journeyInstance;
        private readonly IProviderOwnershipCache _providerOwnershipCache;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(
            JourneyInstance<EditVenueJourneyModel> journeyInstance,
            IProviderOwnershipCache providerOwnershipCache,
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _journeyInstance = journeyInstance;
            _providerOwnershipCache = providerOwnershipCache;
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public Task<Command> Handle(Query request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Command()
            {
                VenueId = request.VenueId,
                ProviderVenueRef = _journeyInstance.State.ProviderVenueRef
            });
        }

        public async Task<OneOf<ModelWithErrors<Command>, Success>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var providerId = await _providerOwnershipCache.GetProviderForVenue(request.VenueId);

            var validator = new CommandValidator(providerId.Value, request.VenueId, _sqlQueryDispatcher);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<Command>(request, validationResult);
            }

            _journeyInstance.UpdateState(state => state.ProviderVenueRef = request.ProviderVenueRef.Trim());

            return new Success();
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(
                Guid providerId,
                Guid venueId,
                ISqlQueryDispatcher sqlQueryDispatcher)
            {
                RuleFor(c => c.ProviderVenueRef)
                    .ProviderVenueRef(providerId, venueId, sqlQueryDispatcher);
            }
        }
    }
}
