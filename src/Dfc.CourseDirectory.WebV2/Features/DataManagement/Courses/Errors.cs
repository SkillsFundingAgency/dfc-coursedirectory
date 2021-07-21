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
        public IReadOnlyCollection<ViewModelErrorRowGroup> ErrorRows { get; set; }
        public bool CanResolveOnScreen { get; set; }
        public int ErrorRowCount { get; set; }
        public int TotalRowCount { get; set; }
    }

    public class ViewModelErrorRowGroup
    {
        public Guid CourseId { get; set; }
        public string LearnAimRef { get; set; }
        public IReadOnlyCollection<ViewModelErrorRow> CourseRows { get; set; }
        public IReadOnlyCollection<string> ErrorFields { get; set; }
    }

    public class ViewModelErrorRow
    {
        public string CourseName { get; set; }
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
            var (errorRows, totalRows) = await _fileUploadProcessor.GetCourseUploadRowsWithErrorsForProvider(
                _providerContextProvider.GetProviderId());

            if (errorRows.Count == 0)
            {
                return new UploadHasNoErrors();
            }

            return CreateViewModel(errorRows, totalRows);
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                var (errorRows, totalRows) = await _fileUploadProcessor.GetCourseUploadRowsWithErrorsForProvider(
                    _providerContextProvider.GetProviderId());

                var vm = CreateViewModel(errorRows, totalRows);
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            return new Success();
        }

        private ViewModel CreateViewModel(IReadOnlyCollection<CourseUploadRow> rows, int totalRows)
        {
            var errorRowCount = rows.Count;
            var canResolveOnScreen = errorRowCount <= 30;

            return new ViewModel()
            {
                ErrorRows = rows
                    .Select(r =>
                    {
                        var errorsByComponent = r.Errors
                            .Select(e => (ErrorCode: e, Field: Core.DataManagement.Errors.MapCourseErrorToFieldGroup(e)))
                            .GroupBy(t => Core.DataManagement.Errors.GetCourseErrorComponent(t.ErrorCode))
                            .ToDictionary(g => g.Key, g => g.Select(i => i.Field).ToArray());

                        return (
                            Row: r,
                            GroupErrorFields: errorsByComponent.GetValueOrDefault(CourseErrorComponent.Course, Array.Empty<string>()),
                            NonGroupErrorFields: errorsByComponent.GetValueOrDefault(CourseErrorComponent.CourseRun, Array.Empty<string>())
                        );
                    })
                    .GroupBy(t => t.Row.CourseId)
                    .Select(g => new ViewModelErrorRowGroup()
                    {
                        CourseId = g.Key,
                        LearnAimRef = g.Select(r => r.Row.LarsQan).Distinct().Single(),
                        CourseRows = g
                            .Select(r => new ViewModelErrorRow()
                            {
                                CourseName = r.Row.CourseName,
                                ProviderCourseRef = r.Row.ProviderCourseRef,
                                StartDate = r.Row.StartDate,
                                VenueName = r.Row.VenueName,
                                DeliveryMode = r.Row.DeliveryMode,
                                ErrorFields = r.NonGroupErrorFields
                            })
                            .Where(r => r.ErrorFields.Count > 0)
                            .OrderByDescending(r => r.ErrorFields.Contains("Delivery mode") ? 1 : 0)
                            .ThenBy(r => r.StartDate)
                            .ThenBy(r => r.DeliveryMode)
                            .ToArray(),
                        ErrorFields = g.First().GroupErrorFields
                    })
                    .OrderByDescending(g => g.CourseRows.Any(r => r.ErrorFields.Contains("Delivery mode")) ? 1 : 0)
                    .ThenByDescending(g => g.ErrorFields.Contains("Course description") ? 1 : 0)
                    .ThenBy(g => g.LearnAimRef)
                    .ThenBy(g => g.CourseId)
                    .ToArray(),
                CanResolveOnScreen = canResolveOnScreen,
                ErrorRowCount = errorRowCount,
                TotalRowCount = totalRows
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
