using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues.Upload
{
    public class Command : IRequest<OneOf<UploadFailedResult, UploadSucceededResult>>
    {
        public IFormFile File { get; set; }
    }

    public enum UploadSucceededResult
    {
        ProcessingInProgress,
        ProcessingCompleted
    }

    public class UploadFailedResult : ModelWithErrors<Command>
    {
        public UploadFailedResult(Command model, ValidationResult validationResult)
            : base(model, validationResult)
        {
            MissingHeaders = Array.Empty<string>();
        }

        public UploadFailedResult(Command model, string fileErrorMessage, IEnumerable<string> missingHeaders = null)
            : base(model, CreateValidationResult(fileErrorMessage))
        {
            MissingHeaders = missingHeaders?.ToArray() ?? Array.Empty<string>();
        }

        public IReadOnlyCollection<string> MissingHeaders { get; }

        private static ValidationResult CreateValidationResult(string fileErrorMessage) =>
            new ValidationResult(new[]
            {
                new ValidationFailure(nameof(Command.File), fileErrorMessage)
            });
    }

    public class Handler : IRequestHandler<Command, OneOf<UploadFailedResult, UploadSucceededResult>>
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

        public async Task<OneOf<UploadFailedResult, UploadSucceededResult>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var result = await validator.ValidateAsync(request);

            if (!result.IsValid)
            {
                return new UploadFailedResult(request, result);
            }

            using var stream = request.File.OpenReadStream();

            var saveFileResult = await _fileUploadProcessor.SaveVenueFile(
                _providerContextProvider.GetProviderId(),
                stream,
                _currentUserProvider.GetCurrentUser());

            if (saveFileResult.Status == SaveFileResultStatus.InvalidFile)
            {
                return new UploadFailedResult(
                    request,
                    "The selected file must be a CSV");
            }
            else if (saveFileResult.Status == SaveFileResultStatus.InvalidRows)
            {
                return new UploadFailedResult(
                    request,
                    "The selected file must use the template");
            }
            else if (saveFileResult.Status == SaveFileResultStatus.InvalidHeader)
            {
                return new UploadFailedResult(
                    request,
                    "Enter headings in the correct format",
                    saveFileResult.MissingHeaders);
            }
            else if (saveFileResult.Status == SaveFileResultStatus.EmptyFile)
            {
                return new UploadFailedResult(
                    request,
                    "The selected file is empty");
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
                await _fileUploadProcessor.WaitForVenueProcessingToComplete(saveFileResult.VenueUploadId, cts.Token);
                return UploadSucceededResult.ProcessingCompleted;
            }
            catch (OperationCanceledException)
            {
                return UploadSucceededResult.ProcessingInProgress;
            }
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.File)
                .NotNull()
                    .WithMessage("Select a CSV")
                .Must(file => file == null || file.Length <= Constants.VenueFileMaxSizeBytes)
                    .WithMessage($"The selected file must be smaller than {Constants.VenueFileMaxSizeLabel}");
        }
    }
}
