using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
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
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Providers.Upload
{
    public class Query : IRequest<ViewModel>
    {
    }

    public class ViewModel : Command
    {
        public int ProviderCount { get; set; }
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
            var result = new ValidationResult { Errors = new List<ValidationFailure>()};//await validator.ValidateAsync(request);

            if (!result.IsValid)
            {
                return new UploadFailedResult(await CreateViewModel(), result);
            }

            using var stream = request.File.OpenReadStream();
            // Test
           // await _fileUploadProcessor.ProcessProviderFile(new Guid("5803717E-E63F-47C0-BEE4-EA14077B941C"), stream);


            var saveFileResult = await _fileUploadProcessor.SaveProviderFile(
                                stream,
                _currentUserProvider.GetCurrentUser());

            if (saveFileResult.Status == SaveProviderFileResultStatus.InvalidFile)
            {
                return new UploadFailedResult(
                    await CreateViewModel(),
                    "The selected file must be a CSV");
            }
            else if (saveFileResult.Status == SaveProviderFileResultStatus.InvalidRows)
            {
                return new UploadFailedResult(
                    await CreateViewModel(),
                    "The selected file must use the template");
            }
            else if (saveFileResult.Status == SaveProviderFileResultStatus.InvalidHeader)
            {
                return new UploadFailedResult(
                    await CreateViewModel(),
                    "Enter headings in the correct format",
                    saveFileResult.MissingHeaders);
            }
            else if (saveFileResult.Status == SaveProviderFileResultStatus.EmptyFile)
            {
                return new UploadFailedResult(
                    await CreateViewModel(),
                    "The selected file is empty");
            }

            return UploadSucceededResult.ProcessingCompletedSuccessfully;
        }

        private async Task<ViewModel> CreateViewModel()
        {
           var model =  await Task.Run(() =>
            {
                return new ViewModel()
                {
                    ProviderCount = 0,
                };

            });
            return model;
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.File)
                .NotNull()
                    .WithMessage("Select a CSV")
                .Must(file => file == null || file.Length <= Constants.CourseFileMaxSizeBytes)
                    .WithMessage($"The selected file must be smaller than {Constants.CourseFileMaxSizeLabel}");
        }
    }
}
