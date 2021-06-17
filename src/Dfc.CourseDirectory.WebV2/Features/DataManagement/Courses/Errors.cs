using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using FluentValidation;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.Errors
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
        public IReadOnlyCollection<ViewModelRow> ErrorRows { get; set; }
        public bool CanResolveOnScreen { get; set; }
        public int ErrorRowCount { get; set; }
        public int TotalRowCount { get; set; }
    }

    public class ViewModelRow
    {
        public string ProviderVenueReference { get; set; }
        public string CourseName { get; set; }
        public IReadOnlyCollection<string> AddressParts { get; set; }
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

        public Handler(
            IProviderContextProvider providerContextProvider,
            IFileUploadProcessor fileUploadProcessor)
        {
            _providerContextProvider = providerContextProvider;
            _fileUploadProcessor = fileUploadProcessor;
        }

        public async Task<OneOf<UploadHasNoErrors, ViewModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            var (uploadRows, uploadStatus) = await _fileUploadProcessor.GetVenueUploadRowsForProvider(
                _providerContextProvider.GetProviderId());

            if (uploadStatus == UploadStatus.ProcessedSuccessfully)
            {
                return new UploadHasNoErrors();
            }

            return CreateViewModel(uploadRows);
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                var (uploadRows, _) = await _fileUploadProcessor.GetVenueUploadRowsForProvider(
                    _providerContextProvider.GetProviderId());

                var vm = CreateViewModel(uploadRows);
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            return new Success();
        }

        private ViewModel CreateViewModel(IReadOnlyCollection<VenueUploadRow> rows)
        {
            var errorRows = rows.Where(row => !row.IsValid).ToArray();
            var errorRowCount = errorRows.Length;
            var canResolveOnScreen = errorRowCount <= 30;

            return new ViewModel()
            {
                ErrorRows = errorRows
                    .Select(row => new ViewModelRow()
                    {
                        ProviderVenueReference = row.ProviderVenueRef,
                        CourseName = row.CourseName,
                        AddressParts =
                            new[]
                            {
                                row.AddressLine1,
                                row.AddressLine2,
                                row.Town,
                                row.County,
                                row.Postcode
                            }
                            .Where(part => !string.IsNullOrWhiteSpace(part))
                            .ToArray(),
                        ErrorFields = row.Errors.Select(e => Core.DataManagement.Errors.MapVenueErrorToFieldGroup(e)).Distinct().ToArray()
                    })
                    .ToArray(),
                CanResolveOnScreen = canResolveOnScreen,
                ErrorRowCount = errorRowCount,
                TotalRowCount = rows.Count
            };
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.WhatNext)
                    .Must(v => v.HasValue && Enum.IsDefined(typeof(WhatNext), v.Value))
                        .WithMessage("Select what you want to do");
            }
        }
    }
}
