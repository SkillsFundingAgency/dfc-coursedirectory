using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.Security;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.Upload
{
    public class Query : IRequest<ViewModel>
    {
    }

    public class ViewModel : Command
    {
        public int CourseCount { get; set; }
    }

    public class Command : IRequest<OneOf<UploadFailedResult, Success>>
    {
        public IFormFile File { get; set; }
    }

    public class UploadFailedResult : ModelWithErrors<ViewModel>
    {
        public UploadFailedResult(ViewModel model, ValidationResult validationResult)
            : base(model, validationResult)
        {
            MissingHeaders = Array.Empty<string>();
            MissingLearnAimRefs = Array.Empty<int>();
            InvalidLearnAimRefs = Array.Empty<(string LearnAimRef, int RowNumber)>();
            ExpiredLearnAimRefs = Array.Empty<(string LearnAimRef, int RowNumber)>();
        }

        public UploadFailedResult(
                ViewModel model,
                string fileErrorMessage,
                IEnumerable<string> missingHeaders = null,
                IEnumerable<int> missingLearnAimRefs = null,
                IEnumerable<(string LearnAimRef, int RowNumber)> invalidLearnAimRefs = null,
                IEnumerable<(string LearnAimRef, int RowNumber)> expiredLearnAimRefs = null)
            : base(model, CreateValidationResult(fileErrorMessage))
        {
            MissingHeaders = missingHeaders?.ToArray() ?? Array.Empty<string>();
            MissingLearnAimRefs = missingLearnAimRefs?.ToArray() ?? Array.Empty<int>();
            InvalidLearnAimRefs = invalidLearnAimRefs?.ToArray() ?? Array.Empty<(string LearnAimRef, int RowNumber)>();
            ExpiredLearnAimRefs = expiredLearnAimRefs?.ToArray() ?? Array.Empty<(string LearnAimRef, int RowNumber)>();
        }

        public IReadOnlyCollection<string> MissingHeaders { get; }
        public IReadOnlyCollection<int> MissingLearnAimRefs { get; }
        public IReadOnlyCollection<(string LearnAimRef, int RowNumber)> InvalidLearnAimRefs { get; }
        public IReadOnlyCollection<(string LearnAimRef, int RowNumber)> ExpiredLearnAimRefs { get; }

        private static ValidationResult CreateValidationResult(string fileErrorMessage) =>
            new ValidationResult(new[]
            {
                new ValidationFailure(nameof(Command.File), fileErrorMessage)
            });
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, OneOf<UploadFailedResult, Success>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ICurrentUserProvider _currentUserProvider;

        public Handler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            IFileUploadProcessor fileUploadProcessor,
            IProviderContextProvider providerContextProvider,
            ICurrentUserProvider currentUserProvider)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _fileUploadProcessor = fileUploadProcessor;
            _providerContextProvider = providerContextProvider;
            _currentUserProvider = currentUserProvider;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken) => CreateViewModel();

        public async Task<OneOf<UploadFailedResult, Success>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var result = await validator.ValidateAsync(request);

            if (!result.IsValid)
            {
                return new UploadFailedResult(await CreateViewModel(), result);
            }

            using var stream = request.File.OpenReadStream();

            var saveFileResult = await _fileUploadProcessor.SaveCourseFile(
                _providerContextProvider.GetProviderId(),
                stream,
                _currentUserProvider.GetCurrentUser());

            if (saveFileResult.Status == SaveCourseFileResultStatus.InvalidFile)
            {
                return new UploadFailedResult(
                    await CreateViewModel(),
                    "The selected file must be a CSV");
            }
            else if (saveFileResult.Status == SaveCourseFileResultStatus.InvalidRows)
            {
                return new UploadFailedResult(
                    await CreateViewModel(),
                    "The selected file must use the template");
            }
            else if (saveFileResult.Status == SaveCourseFileResultStatus.InvalidHeader)
            {
                return new UploadFailedResult(
                    await CreateViewModel(),
                    "Enter headings in the correct format",
                    saveFileResult.MissingHeaders);
            }
            else if (saveFileResult.Status == SaveCourseFileResultStatus.InvalidLars)
            {
                return new UploadFailedResult(
                    await CreateViewModel(),
                    "The file contains errors",
                    null,
                    saveFileResult.MissingLearnAimRefRows,
                    saveFileResult.InvalidLearnAimRefRows,
                    saveFileResult.ExpiredLearnAimRefRows);
            }
            else if (saveFileResult.Status == SaveCourseFileResultStatus.EmptyFile)
            {
                return new UploadFailedResult(
                    await CreateViewModel(),
                    "The selected file is empty");
            }

            else if (saveFileResult.Status == SaveCourseFileResultStatus.ExistingFileInFlight)
            {
                // UI Should stop us getting here so a generic error is sufficient
                throw new InvalidStateException();
            }

            return new Success();
        }

        private async Task<ViewModel> CreateViewModel()
        {
            var courseRunCount = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLiveCourseRunCountForProvider() { ProviderId = _providerContextProvider.GetProviderId() });

            return new ViewModel()
            {
                CourseCount = courseRunCount
            };
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.File).NotNull().WithMessage("Select a CSV");
                RuleFor(x => x.File)
                    .NotNull()
                        .WithMessage("Select a CSV")
                    .Must(file => file == null || file.Length <= Constants.CourseFileMaxSizeBytes)
                        .WithMessage($"The selected file must be smaller than {Constants.CourseFileMaxSizeLabel}");
            }
        }
    }
}
