using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using FluentValidation;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.ResolveRowDeliveryMode
{
    public class Query : IRequest<OneOf<NotFound, Command>>
    {
        public int RowNumber { get; set; }
    }

    public class Command : IRequest<OneOf<NotFound, ModelWithErrors<Command>, Success>>
    {
        public int RowNumber { get; set; }
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
            var rowStatus = await GetRowStatus(request.RowNumber);

            if (rowStatus == RowStatus.DoesNotExist)
            {
                return new NotFound();
            }

            if (rowStatus == RowStatus.DoesNotHaveErrors)
            {
                throw new InvalidStateException();
            }

            Debug.Assert(rowStatus == RowStatus.ExistsWithErrors);

            return new Command()
            {
                RowNumber = request.RowNumber
            };
        }

        public async Task<OneOf<NotFound, ModelWithErrors<Command>, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var rowStatus = await GetRowStatus(request.RowNumber);

            if (rowStatus == RowStatus.DoesNotExist)
            {
                return new NotFound();
            }

            if (rowStatus == RowStatus.DoesNotHaveErrors)
            {
                throw new InvalidStateException();
            }

            Debug.Assert(rowStatus == RowStatus.ExistsWithErrors);

            var validator = new CommandValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<Command>(request, validationResult);
            }

            return new Success();
        }

        private async Task<RowStatus> GetRowStatus(int rowNumber)
        {
            var providerId = _providerContextProvider.GetProviderId();
            var row = await _fileUploadProcessor.GetCourseUploadRowDetailForProvider(providerId, rowNumber);

            if (row == null)
            {
                return RowStatus.DoesNotExist;
            }

            if (row.IsValid)
            {
                return RowStatus.DoesNotHaveErrors;
            }

            return RowStatus.ExistsWithErrors;
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

        private enum RowStatus
        {
            DoesNotExist,
            DoesNotHaveErrors,
            ExistsWithErrors
        }
    }
}
