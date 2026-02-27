using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Text;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers;
using Dfc.CourseDirectory.Core.Middleware;
using Dfc.CourseDirectory.Core.Security;
using Dfc.CourseDirectory.Core.Validation;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SqlServer.Dac.Model;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.ViewModels.DataManagement.Providers.Upload
{
    public class Query : IRequest<ViewModel>
    {
    }

    public class ViewModel : Command
    {
        public int ProviderCount { get; set; }

    }

    public class Command : IRequest<OneOf<UploadFailedResult, ProviderUploadResult>>
    {
        public IFormFile File { get; set; }
        public bool InactiveProviders { get; set; }
        public int Duration { get; set; }

    }

    public enum UploadSucceededResult
    {
        ProcessingInProgress,
        ProcessingCompletedWithErrors,
        ProcessingCompletedSuccessfully
    }

    public class ProviderUploadResult
    {
        public UploadSucceededResult UploadSucceededResult { get; set; }
        public Guid ProviderUploadId { get; set; }
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
        IRequestHandler<Command, OneOf<UploadFailedResult, ProviderUploadResult>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IOptions<DataManagementOptions> _optionsAccessor;
        private readonly ILogger<Handler> _logger;

        public Handler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            IFileUploadProcessor fileUploadProcessor,
            IProviderContextProvider providerContextProvider,
            ICurrentUserProvider currentUserProvider,
            IOptions<DataManagementOptions> optionsAccessor,
            ILogger<Handler> logger)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _fileUploadProcessor = fileUploadProcessor;
            _providerContextProvider = providerContextProvider;
            _currentUserProvider = currentUserProvider;
            _optionsAccessor = optionsAccessor;
            _logger = logger;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken) => CreateViewModel();

        public async Task<OneOf<UploadFailedResult, ProviderUploadResult>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            try
            {

                var validator = new CommandValidator();
                var result = await validator.ValidateAsync(request);

                if (!result.IsValid)
                {
                    return new UploadFailedResult(await CreateViewModel(), result);
                }

                using var stream = request.File.OpenReadStream();

                var saveFileResult = await _fileUploadProcessor.SaveProviderFile(
                                    stream,
                                    request.InactiveProviders,
                                    request.Duration,
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
                else if (saveFileResult.Status == SaveProviderFileResultStatus.InvalidHeader && request.InactiveProviders)
                {
                    return new UploadFailedResult(
                        await CreateViewModel(),
                       "The selected file does not match the format for an inactive report");
                }
                else if (saveFileResult.Status == SaveProviderFileResultStatus.InvalidHeader && !request.InactiveProviders)
                {
                    return new UploadFailedResult(
                        await CreateViewModel(),
                       "The selected file does not match the format for an active report");
                }
                else if (saveFileResult.Status == SaveProviderFileResultStatus.EmptyFile)
                {
                    return new UploadFailedResult(
                        await CreateViewModel(),
                        "The selected file is empty");
                }

                return new ProviderUploadResult
                { UploadSucceededResult = UploadSucceededResult.ProcessingCompletedSuccessfully, ProviderUploadId = saveFileResult.ProviderUploadId };

            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"Provider upload failed");
                return new ProviderUploadResult
                { UploadSucceededResult = UploadSucceededResult.ProcessingCompletedWithErrors };

            }
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
                   .WithMessage("The selected file must be a CSV")
                .Must(file => file == null || file.Length <= Constants.ProviderFileMaxSizeBytes)
                    .WithMessage($"The selected file must be smaller than {Constants.ProviderFileMaxSizeLabel}");
        }
    }
}
