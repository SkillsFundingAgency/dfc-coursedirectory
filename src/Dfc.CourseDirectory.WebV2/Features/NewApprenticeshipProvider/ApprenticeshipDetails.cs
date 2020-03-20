using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.Validation;
using Dfc.CourseDirectory.WebV2.Validation.ApprenticeshipValidation;
using FluentValidation;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider.ApprenticeshipDetails
{
    using CommandResponse = OneOf<ModelWithErrors<Command>, Success>;

    public class Query : IRequest<Command>
    {
        public Guid ProviderId { get; set; }
        public StandardOrFramework StandardOrFramework { get; set; }
    }

    public class Command : IRequest<CommandResponse>
    {
        public Guid ProviderId { get; set; }
        public StandardOrFramework StandardOrFramework { get; set; }
        public string MarketingInformation { get; set; }
        public string Website { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactWebsite { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, Command>,
        IRestrictProviderType<Query>,
        IRequireUserCanSubmitQASubmission<Query>,
        IRestrictQAStatus<Query>,
        IRequestHandler<Command, CommandResponse>,
        IRestrictProviderType<Command>,
        IRequireUserCanSubmitQASubmission<Command>,
        IRestrictQAStatus<Command>
    {
        public Handler()
        {
        }

        public IEnumerable<ApprenticeshipQAStatus> PermittedStatuses { get; } = new[]
        {
            ApprenticeshipQAStatus.NotStarted
        };

        public ProviderType ProviderType => ProviderType.Apprenticeships;

        public Task<Command> Handle(Query request, CancellationToken cancellationToken)
        {
            var command = new Command()
            {
                ProviderId = request.ProviderId,
                StandardOrFramework = request.StandardOrFramework
            };

            return Task.FromResult(command);
        }

        public async Task<CommandResponse> Handle(Command request, CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<Command>(request, validationResult);
            }

            return new Success();
        }

        Task<Guid> IRestrictProviderType<Query>.GetProviderId(Query request) =>
            Task.FromResult(request.ProviderId);

        Task<Guid> IRestrictProviderType<Command>.GetProviderId(Command request) =>
            Task.FromResult(request.ProviderId);

        Task<Guid> IRestrictQAStatus<Query>.GetProviderId(Query request) =>
            Task.FromResult(request.ProviderId);

        Task<Guid> IRestrictQAStatus<Command>.GetProviderId(Command request) =>
            Task.FromResult(request.ProviderId);

        Task<Guid> IRequireUserCanSubmitQASubmission<Query>.GetProviderId(Query request) =>
            Task.FromResult(request.ProviderId);

        Task<Guid> IRequireUserCanSubmitQASubmission<Command>.GetProviderId(Command request) =>
            Task.FromResult(request.ProviderId);

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(m => m.MarketingInformation).MarketingInformation();
                RuleFor(m => m.Website).Website();
                RuleFor(m => m.ContactTelephone).ContactTelephone();
                RuleFor(m => m.ContactEmail).ContactEmail();
                RuleFor(m => m.ContactWebsite).ContactWebsite();
            }
        }
    }
}
