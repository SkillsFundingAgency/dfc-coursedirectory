using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
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

        public Handler(IFileUploadProcessor fileUploadProcessor, IProviderContextProvider providerContextProvider)
        {
            _fileUploadProcessor = fileUploadProcessor;
            _providerContextProvider = providerContextProvider;
        }

        public async Task<OneOf<NotFound, Command>> Handle(Query request, CancellationToken cancellationToken)
        {
            return new Command();
        }


        public async Task<OneOf<NotFound, ModelWithErrors<Command>, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<Command>(request, validationResult);
            }

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
