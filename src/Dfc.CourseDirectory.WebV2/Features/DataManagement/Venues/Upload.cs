using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
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
    public class Query : IRequest<ViewModel>
    {
    }

    public class ViewModel : Command
    {
        public int VenueCount { get; set; }
    }

    public class Command : IRequest<OneOf<UploadFailedResult, UploadSucceededResult>>
    {
        public IFormFile File { get; set; }
    }

    public enum UploadSucceededResult
    {
        ProcessingInProgress,
        ProcessingCompletedWithErrors,
        ProcessingCompletedSuccessfully
    }

    public class UploadFailedResult : ModelWithErrors<ViewModel>
    {
        public UploadFailedResult(ViewModel model, ValidationResult validationResult)
            : base(model, validationResult)
        {
            MissingHeaders = Array.Empty<string>();
        }

        public UploadFailedResult(ViewModel model, string fileErrorMessage, IEnumerable<string> missingHeaders = null)
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

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, OneOf<UploadFailedResult, UploadSucceededResult>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IOptions<DataManagementOptions> _optionsAccessor;

        public Handler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            IFileUploadProcessor fileUploadProcessor,
            IProviderContextProvider providerContextProvider,
            ICurrentUserProvider currentUserProvider,
            IOptions<DataManagementOptions> optionsAccessor)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _fileUploadProcessor = fileUploadProcessor;
            _providerContextProvider = providerContextProvider;
            _currentUserProvider = currentUserProvider;
            _optionsAccessor = optionsAccessor;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken) => CreateViewModel();

        public async Task<OneOf<UploadFailedResult, UploadSucceededResult>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var result = await validator.ValidateAsync(request);

            if (!result.IsValid)
            {
                return new UploadFailedResult(await CreateViewModel(), result);
            }

            var providerId = _providerContextProvider.GetProviderId();

            using var stream = request.File.OpenReadStream();

            var saveFileResult = await _fileUploadProcessor.SaveVenueFile(
                providerId,
                stream,
                _currentUserProvider.GetCurrentUser());

            if (saveFileResult.Status == SaveVenueFileResultStatus.InvalidFile)
            {
                return new UploadFailedResult(
                    await CreateViewModel(),
                    "The selected file must be a CSV");
            }
            else if (saveFileResult.Status == SaveVenueFileResultStatus.InvalidRows)
            {
                return new UploadFailedResult(
                    await CreateViewModel(),
                    "The selected file must use the template");
            }
            else if (saveFileResult.Status == SaveVenueFileResultStatus.InvalidHeader)
            {
                return new UploadFailedResult(
                    await CreateViewModel(),
                    "Enter headings in the correct format",
                    saveFileResult.MissingHeaders);
            }
            else if (saveFileResult.Status == SaveVenueFileResultStatus.EmptyFile)
            {
                return new UploadFailedResult(
                    await CreateViewModel(),
                    "The selected file is empty");
            }
            else if (saveFileResult.Status == SaveVenueFileResultStatus.ExistingFileInFlight)
            {
                // UI Should stop us getting here so a generic error is sufficient
                throw new InvalidStateException();
            }

            Debug.Assert(saveFileResult.Status == SaveVenueFileResultStatus.Success);

            // Wait for a little bit to see if the file gets processed quickly
            // (so we can skip the In Progress UI)

            try
            {
                using var cts = new CancellationTokenSource(_optionsAccessor.Value.ProcessedImmediatelyThreshold);
                var uploadStatus = await _fileUploadProcessor.WaitForVenueProcessingToCompleteForProvider(providerId, cts.Token);

                return uploadStatus == UploadStatus.ProcessedWithErrors ?
                    UploadSucceededResult.ProcessingCompletedWithErrors :
                    UploadSucceededResult.ProcessingCompletedSuccessfully;
            }
            catch (OperationCanceledException)
            {
                return UploadSucceededResult.ProcessingInProgress;
            }
        }

        private async Task<ViewModel> CreateViewModel()
        {
            var venues = await _sqlQueryDispatcher.ExecuteQuery(
                new GetVenuesByProvider() { ProviderId = _providerContextProvider.GetProviderId() });

            return new ViewModel()
            {
                VenueCount = venues.Count()
            };
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
