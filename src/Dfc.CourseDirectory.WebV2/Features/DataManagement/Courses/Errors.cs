using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
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
        public int ErrorCount { get; set; }
    }

    public class ViewModelErrorRowGroup
    {
        public Guid CourseId { get; set; }
        public string LearnAimRef { get; set; }
        public string LearnAimRefTitle { get; set; }
        public IReadOnlyCollection<ViewModelErrorRow> CourseRows { get; set; }
        public IReadOnlyCollection<string> ErrorFields { get; set; }
    }

    public class ViewModelErrorRow
    {
        public string CourseName { get; set; }
        public string StartDate { get; set; }
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
            var errorRows = await _fileUploadProcessor.GetCourseUploadRowsWithErrorsForProvider(
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
                var errorRows = await _fileUploadProcessor.GetCourseUploadRowsWithErrorsForProvider(
                    _providerContextProvider.GetProviderId());

                var vm = await CreateViewModel(errorRows);
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            return new Success();
        }

        private async Task<ViewModel> CreateViewModel(IReadOnlyCollection<CourseUploadRow> rows)
        {
            var errorRowCount = rows.Count;
            var canResolveOnScreen = errorRowCount <= 30;
            var errorCount = rows.SelectMany(r => r.Errors).Count();

            var learnAimRefs = rows.Select(r => r.LearnAimRef).Distinct();
            var learningDeliveries = await _sqlQueryDispatcher.ExecuteQuery(new GetLearningDeliveries() { LearnAimRefs = learnAimRefs });

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
                    .Select(g =>
                    {
                        var learnAimRef = g.Select(r => r.Row.LearnAimRef).Distinct().Single();

                        return new ViewModelErrorRowGroup()
                        {
                            CourseId = g.Key,
                            LearnAimRef = learnAimRef,
                            LearnAimRefTitle = learningDeliveries[learnAimRef].LearnAimRefTitle,
                            CourseRows = g
                                .Select(r => new ViewModelErrorRow()
                                {
                                    CourseName = r.Row.CourseName,
                                    StartDate = r.Row.StartDate,
                                    DeliveryMode = r.Row.DeliveryMode,
                                    ErrorFields = r.NonGroupErrorFields
                                })
                                .Where(r => r.ErrorFields.Count > 0)
                                .OrderByDescending(r => r.ErrorFields.Contains("Delivery mode") ? 1 : 0)
                                .ThenBy(r => r.StartDate)
                                .ThenBy(r => r.DeliveryMode)
                                .ToArray(),
                            ErrorFields = g.First().GroupErrorFields
                        };
                    })
                    .OrderByDescending(g => g.CourseRows.Any(r => r.ErrorFields.Contains("Delivery mode")) ? 1 : 0)
                    .ThenByDescending(g => g.ErrorFields.Contains("Course description") ? 1 : 0)
                    .ThenBy(g => g.LearnAimRef)
                    .ThenBy(g => g.CourseId)
                    .ToArray(),
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
