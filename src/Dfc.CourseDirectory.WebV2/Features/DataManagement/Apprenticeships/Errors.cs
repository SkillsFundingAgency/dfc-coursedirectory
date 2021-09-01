using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Validation;
using FluentValidation;
using MediatR;
using OneOf;
using OneOf.Types;
using System.Linq;
using System;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Apprenticeships.Errors
{
    public class Query : IRequest<OneOf<UploadHasNoErrors, ViewModel>>
    {
    }

    public struct UploadHasNoErrors { }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        public WhatNext? WhatNext { get; set; }
    }

    public class ViewModel : Command
    {
        public IReadOnlyCollection<ViewModelErrorRow> ErrorRows { get; set; }
        public bool CanResolveOnScreen { get; set; }
        public int ErrorCount { get; set; }
    }


    public class ViewModelErrorRow
    {
        public string Row { get; set; }
        public int Code { get; set; }
        public int Version { get; set; }
        public string ApprenticeshipName { get; set; }
        public string DeliveryMode { get; set; }
        public IReadOnlyCollection<string> ErrorFields { get; set; }
    }

    public enum WhatNext
    {
        ResolveOnScreen,
        UploadNewFile,
        DeleteUpload
    }

    public class Handler :
        IRequestHandler<Query, OneOf<UploadHasNoErrors, ViewModel>>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(
            IProviderContextProvider providerContextProvider,
            IFileUploadProcessor fileUploadProcessor,
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _providerContextProvider = providerContextProvider;
            _fileUploadProcessor = fileUploadProcessor;
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<OneOf<UploadHasNoErrors, ViewModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            var errorRows = await _fileUploadProcessor.GetApprenticeshipUploadRowsWithErrorsForProvider(
                _providerContextProvider.GetProviderId());

            if (errorRows.Count == 0)
            {
                return new UploadHasNoErrors();
            }

            return await CreateViewModel(errorRows);
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                var errorRows = await _fileUploadProcessor.GetApprenticeshipUploadRowsWithErrorsForProvider(
                    _providerContextProvider.GetProviderId());

                var vm = await CreateViewModel(errorRows);
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            return new Success();
        }


        private async Task<ViewModel> CreateViewModel(IReadOnlyCollection<ApprenticeshipUploadRow> rows)
        {
            var errorRowCount = rows.Count;
            var canResolveOnScreen = errorRowCount <= 30;
            var errorCount = rows.SelectMany(r => r.Errors).Count();

            return new ViewModel()
            {
                ErrorRows = rows
                                .Select(r =>
                                {
                                    return new ViewModelErrorRow()
                                    {
                                        Row = r.RowNumber.ToString(),
                                        Code = r.StandardCode,
                                        Version = r.StandardVersion,
                                        ApprenticeshipName = r.ApprenticeshipInformation,
                                        DeliveryMode = r.DeliveryMode,
                                        ErrorFields = r.Errors.Select(e => Core.DataManagement.Errors.MapApprenticeshipErrorToFieldGroup(e)).Distinct().ToArray()
                                    };
                                }).ToArray(),
                CanResolveOnScreen = canResolveOnScreen,
                ErrorCount = errorCount
            };
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.WhatNext)
                    .NotEmpty().IsInEnum()
                        .WithMessageForAllRules("Select what you want to do");
            }
        }
    }
}
