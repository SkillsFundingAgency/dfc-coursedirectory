﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.ApprenticeshipValidation;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using FluentValidation;
using Mapster;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider.ApprenticeshipDetails
{
    using CommandResponse = OneOf<ModelWithErrors<Command>, Success>;

    public class Query : IRequest<ViewModel>
    {
        public Guid ProviderId { get; set; }
    }

    public class Command : IRequest<CommandResponse>
    {
        public Guid ProviderId { get; set; }
        public string MarketingInformation { get; set; }
        public string Website { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactWebsite { get; set; }
    }

    public class ViewModel : Command
    {
        public StandardOrFramework StandardOrFramework { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, CommandResponse>
    {
        private readonly MptxInstanceContext<FlowModel> _flow;

        public Handler(MptxInstanceContext<FlowModel> flow)
        {
            _flow = flow;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            if (_flow.State.ApprenticeshipStandardOrFramework == null)
            {
                throw new InvalidStateException();
            }

            var vm = new ViewModel()
            {
                ProviderId = request.ProviderId,
                StandardOrFramework = _flow.State.ApprenticeshipStandardOrFramework,
                MarketingInformation = _flow.State.ApprenticeshipMarketingInformation,
                Website = _flow.State.ApprenticeshipContactWebsite,
                ContactTelephone = _flow.State.ApprenticeshipContactTelephone,
                ContactEmail = _flow.State.ApprenticeshipContactEmail,
                ContactWebsite = _flow.State.ApprenticeshipContactWebsite
            };
            return Task.FromResult(vm);
        }

        public async Task<CommandResponse> Handle(Command request, CancellationToken cancellationToken)
        {
            if (_flow.State.ApprenticeshipStandardOrFramework == null)
            {
                throw new InvalidStateException();
            }

            var validator = new CommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var vm = request.Adapt<ViewModel>();
                vm.StandardOrFramework = _flow.State.ApprenticeshipStandardOrFramework;

                return new ModelWithErrors<Command>(vm, validationResult);
            }

            _flow.Update(s => s.SetApprenticeshipDetails(
                request.MarketingInformation,
                request.Website,
                request.ContactTelephone,
                request.ContactEmail,
                request.ContactWebsite));

            return new Success();
        }

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
