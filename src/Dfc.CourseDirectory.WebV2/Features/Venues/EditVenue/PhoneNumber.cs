using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.VenueValidation;
using FluentValidation;
using FormFlow;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.Venues.EditVenue.PhoneNumber
{
    public class Query : IRequest<Command>
    {
        public Guid VenueId { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<Command>, Success>>
    {
        public Guid VenueId { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, Command>,
        IRequestHandler<Command, OneOf<ModelWithErrors<Command>, Success>>
    {
        private readonly JourneyInstance<EditVenueJourneyModel> _journeyInstance;

        public Handler(JourneyInstance<EditVenueJourneyModel> journeyInstance)
        {
            _journeyInstance = journeyInstance;
        }

        public Task<Command> Handle(Query request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Command()
            {
                VenueId = request.VenueId,
                PhoneNumber = _journeyInstance.State.PhoneNumber
            });
        }

        public async Task<OneOf<ModelWithErrors<Command>, Success>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<Command>(request, validationResult);
            }

            _journeyInstance.UpdateState(state => state.PhoneNumber = request.PhoneNumber ?? string.Empty);

            return new Success();
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.PhoneNumber).PhoneNumber();
            }
        }
    }
}
