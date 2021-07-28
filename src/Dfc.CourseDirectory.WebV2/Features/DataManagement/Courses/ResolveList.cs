using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using MediatR;
using OneOf;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.ResolveList
{
    public class Query : IRequest<OneOf<UploadHasNoErrors, ViewModel>>
    {
    }

    public struct UploadHasNoErrors { }

    public class ViewModel
    {
        public IReadOnlyCollection<ViewModelErrorRowGroup> ErrorRows { get; set; }
    }

    public class ViewModelErrorRowGroup
    {
        public int RowNumber { get; set; }
        public Guid CourseId { get; set; }
        public string LearnAimRef { get; set; }
        public string LearnAimRefTitle { get; set; }
        public IReadOnlyCollection<ViewModelErrorRow> CourseRows { get; set; }
        public IReadOnlyCollection<string> ErrorFields { get; set; }
        public bool HasDescriptionErrors { get; set; }
    }

    public class ViewModelErrorRow
    {
        public int RowNumber { get; set; }
        public string CourseName { get; set; }
        public string ProviderCourseRef { get; set; }
        public string StartDate { get; set; }
        public string VenueName { get; set; }
        public string DeliveryMode { get; set; }
        public IReadOnlyCollection<string> ErrorFields { get; set; }
        public bool HasDeliveryModeError { get; set; }
        public bool HasDetailErrors { get; set; }
    }

    public class Handler : IRequestHandler<Query, OneOf<UploadHasNoErrors, ViewModel>>
    {
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(
            IFileUploadProcessor fileUploadProcessor,
            IProviderContextProvider providerContextProvider,
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _fileUploadProcessor = fileUploadProcessor;
            _providerContextProvider = providerContextProvider;
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

            var learnAimRefs = errorRows.Select(r => r.LearnAimRef).Distinct();
            var learningDeliveries = await _sqlQueryDispatcher.ExecuteQuery(new GetLearningDeliveries() { LearnAimRefs = learnAimRefs });

            return new ViewModel()
            {
                ErrorRows = errorRows
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
                            RowNumber = g.First().Row.RowNumber,
                            CourseId = g.Key,
                            LearnAimRef = learnAimRef,
                            LearnAimRefTitle = learningDeliveries[learnAimRef].LearnAimRefTitle,
                            CourseRows = g
                                .Select(r => new ViewModelErrorRow()
                                {
                                    RowNumber = r.Row.RowNumber,
                                    CourseName = r.Row.CourseName,
                                    ProviderCourseRef = r.Row.ProviderCourseRef,
                                    StartDate = r.Row.StartDate,
                                    VenueName = r.Row.VenueName,
                                    DeliveryMode = r.Row.DeliveryMode,
                                    ErrorFields = r.NonGroupErrorFields,
                                    HasDeliveryModeError = r.NonGroupErrorFields.Contains("Delivery mode"),
                                    HasDetailErrors = r.NonGroupErrorFields.Except(new[] { "Delivery mode" }).Any()
                                })
                                .Where(r => r.ErrorFields.Count > 0)
                                .OrderByDescending(r => r.ErrorFields.Contains("Delivery mode") ? 1 : 0)
                                .ThenBy(r => r.StartDate)
                                .ThenBy(r => r.DeliveryMode)
                                .ToArray(),
                            ErrorFields = g.First().GroupErrorFields,
                            HasDescriptionErrors = g.First().GroupErrorFields.Any()
                        };
                    })
                    .OrderByDescending(g => g.CourseRows.Any(r => r.ErrorFields.Contains("Delivery mode")) ? 1 : 0)
                    .ThenByDescending(g => g.ErrorFields.Contains("Course description") ? 1 : 0)
                    .ThenBy(g => g.LearnAimRef)
                    .ThenBy(g => g.CourseId)
                    .ToArray()
            };
        }
    }
}
