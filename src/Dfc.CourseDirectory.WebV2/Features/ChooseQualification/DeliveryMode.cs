using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using FluentValidation;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.ChooseQualification.DeliveryMode
{
    public class Query : IRequest<OneOf<NotFound, Command>>
    {
    }

    public class Command : IRequest<OneOf<NotFound, ModelWithErrors<Command>, Success>>
    {
        public CourseDeliveryMode? DeliveryMode { get; set; }
    }

    public class Handler :
    IRequestHandler<Query, OneOf<NotFound, Command>>,
    IRequestHandler<Command, OneOf<NotFound, ModelWithErrors<Command>, Success>>
    {
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly MptxInstanceContext<FlowModel> _flow;

        public Handler()
        {
         
        }

        public Handler(IFileUploadProcessor fileUploadProcessor, IProviderContextProvider providerContextProvider, MptxInstanceContext<FlowModel> flow)
        {
            _fileUploadProcessor = fileUploadProcessor;
            _providerContextProvider = providerContextProvider;
            _flow = flow;
        }

        public async Task<OneOf<NotFound, Command>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (_flow.State.LarsCode == null || string.IsNullOrEmpty(_flow.State.WhoThisCourseIsFor))
            {
                throw new InvalidStateException();
            }

            var item = new Command()
            {
                DeliveryMode = _flow.State.DeliveryMode
            };
            return await Task.FromResult(item);
        }

        public async Task<OneOf<NotFound, ModelWithErrors<Command>, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (_flow.State.LarsCode == null)
            {
                throw new InvalidStateException();
            }
            var validator = new CommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<Command>(request, validationResult);
            }

            _flow.Update(s => s.SetDeliveryMode(request.DeliveryMode));
            return new Success();
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.DeliveryMode)
                    .Must(v => v.HasValue && Enum.IsDefined(typeof(CourseDeliveryMode), v))
                        .WithMessage("Select how the course will be delivered");
            }
        }

    }
}
