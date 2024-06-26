﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Services;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.VenueValidation;
using FluentValidation;
using FormFlow;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.Venues.EditVenue.Website
{
    public class Query : IRequest<Command>
    {
        public Guid VenueId { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<Command>, Success>>
    {
        public Guid VenueId { get; set; }
        public string Website { get; set; }
        public bool IsSecureWebsite { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, Command>,
        IRequestHandler<Command, OneOf<ModelWithErrors<Command>, Success>>
    {
        private readonly JourneyInstance<EditVenueJourneyModel> _journeyInstance;
        private readonly IWebRiskService _webRiskService;

        public Handler(JourneyInstance<EditVenueJourneyModel> journeyInstance, IWebRiskService webRiskService)
        {
            _journeyInstance = journeyInstance;
            _webRiskService = webRiskService;
        }

        public Task<Command> Handle(Query request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Command()
            {
                VenueId = request.VenueId,
                Website = _journeyInstance.State.Website
            });
        }

        public async Task<OneOf<ModelWithErrors<Command>, Success>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var validator = new CommandValidator(_webRiskService);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<Command>(request, validationResult);
            }

            _journeyInstance.UpdateState(state => state.Website = request.Website?.Trim() ?? string.Empty);

            return new Success();
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(IWebRiskService webRiskService)
            {
                RuleFor(c => c.Website).Website(webRiskService);
            }
        }
    }
}
