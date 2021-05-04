using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.Security;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OneOf;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.Upload
{
    public class Command : IRequest<OneOf<ModelWithErrors<Command>, UploadResult>>
    {
        public IFormFile File { get; set; }
    }

    public enum UploadResult
    {
        ProcessingInProgress,
        ProcessingCompleted
    }

    public class Handler : IRequestHandler<Command, OneOf<ModelWithErrors<Command>, UploadResult>>
    {
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IOptions<DataManagementOptions> _optionsAccessor;

        public Handler(
            IFileUploadProcessor fileUploadProcessor,
            IProviderContextProvider providerContextProvider,
            ICurrentUserProvider currentUserProvider,
            IOptions<DataManagementOptions> optionsAccessor)
        {
            _fileUploadProcessor = fileUploadProcessor;
            _providerContextProvider = providerContextProvider;
            _currentUserProvider = currentUserProvider;
            _optionsAccessor = optionsAccessor;
        }

        public async Task<OneOf<ModelWithErrors<Command>, UploadResult>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var result = await validator.ValidateAsync(request);

            if (!result.IsValid)
            {
                return new ModelWithErrors<Command>(request, result);
            }

            using var stream = request.File.OpenReadStream();

            var saveFileResult = await _fileUploadProcessor.SaveCourseFile(
                _providerContextProvider.GetProviderId(),
                stream,
                _currentUserProvider.GetCurrentUser());

            if (saveFileResult.Status == SaveFileResultStatus.InvalidFile)
            {
                return new ModelWithErrors<Command>(
                    request,
                    CreateValidationResultFromError("The selected file must be a CSV"));
            }
            else if (saveFileResult.Status == SaveFileResultStatus.InvalidHeader)
            {
                // TODO PTCD-920
                throw new NotImplementedException();
            }
            else if (saveFileResult.Status == SaveFileResultStatus.EmptyFile)
            {
                return new ModelWithErrors<Command>(
                    request,
                    CreateValidationResultFromError("The selected file is empty"));
            }
            else if (saveFileResult.Status == SaveFileResultStatus.ExistingFileInFlight)
            {
                // UI Should stop us getting here so a generic error is sufficient
                throw new InvalidStateException();
            }

            Debug.Assert(saveFileResult.Status == SaveFileResultStatus.Success);

            // Wait for a little bit to see if the file gets processed quickly
            // (so we can skip the In Progress UI)

            try
            {
                using var cts = new CancellationTokenSource(_optionsAccessor.Value.ProcessedImmediatelyThreshold);
                return UploadResult.ProcessingCompleted;
            }
            catch (OperationCanceledException)
            {
                return UploadResult.ProcessingInProgress;
            }

            static ValidationResult CreateValidationResultFromError(string message) =>
                new ValidationResult(new[]
                {
                    new ValidationFailure(nameof(request.File), message)
                });
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.File).NotNull().WithMessage("Select a CSV");
        }
    }
}
