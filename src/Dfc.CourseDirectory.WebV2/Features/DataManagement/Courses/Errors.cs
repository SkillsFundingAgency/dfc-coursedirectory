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
        public IReadOnlyCollection<ViewModelCourse> ErrorRows { get; set; }
        public bool CanResolveOnScreen { get; set; }
        public int ErrorRowCount { get; set; }
        public int TotalRowCount { get; set; }
    }

    public class ViewModelCourse
    {
        public Guid CourseId { get; set; }
        public string LarsQan { get; set; }
        public string CourseName { get; set; }
        public IReadOnlyCollection<ViewModelRow> CourseRows { get; set; }
    }

    public class ViewModelRow
    {
        public string ProviderCourseRef { get; set; }

        public string StartDate { get; set; }
        public string VenueName { get; set; }
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

        public Handler(
            IProviderContextProvider providerContextProvider,
            IFileUploadProcessor fileUploadProcessor)
        {
            _providerContextProvider = providerContextProvider;
            _fileUploadProcessor = fileUploadProcessor;
        }

        public async Task<OneOf<UploadHasNoErrors, ViewModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            var (uploadRows, uploadStatus) = await _fileUploadProcessor.GetCourseUploadRowsForProvider(
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
                var uploadRows = await _fileUploadProcessor.GetCourseUploadRowsWithErrorsForProvider(
                    _providerContextProvider.GetProviderId());

                var vm = CreateViewModel(uploadRows);
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            return new Success();
        }

        private ViewModel CreateViewModel(IReadOnlyCollection<CourseUploadRow> rows)
        {
            var errorRows = rows.Where(row => !row.IsValid).ToArray();
            var errorRowCount = errorRows.Length;
            var canResolveOnScreen = errorRowCount <= 30;

            return new ViewModel()
            {
                ErrorRows = errorRows
                    .GroupBy(row => row.CourseId)
                    .Select(g => new ViewModelCourse()
                    {
                        CourseId = g.Key,
                        LarsQan = g.Select(p=>p.LarsQan).FirstOrDefault(),
                        CourseName = g.Select(p => p.CourseName).FirstOrDefault(),
                        CourseRows = g.Select(r => new ViewModelRow()
                            {
                                ProviderCourseRef = r.ProviderCourseRef,
                                StartDate = r.StartDate,
                                VenueName = r.VenueName,
                                DeliveryMode = r.DeliveryMode,
                                ErrorFields = r.Errors.Select(e => Core.DataManagement.Errors.MapCourseErrorToFieldGroup(e)).Distinct().ToArray()
                            }).OrderBy(
                                    g => g.StartDate
                                ).ThenByDescending(
                                    g => g.DeliveryMode
                                ).ToArray()
                    }).OrderBy(
                        g => g.LarsQan
                    ).ThenByDescending(
                        g => g.CourseId
                    )

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
