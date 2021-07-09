using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Validation;
using FluentValidation.Results;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.DeleteRow
{
    public class Query : IRequest<OneOf<NotFound, Response>>
    {
        public int RowNumber { get; set; }
    }

    public enum DeleteRowResult
    {
        CourseRowDeletedHasNoMoreErrors = 1,
        CourseRowDeletedHasMoreErrors = 2
    }

    public class ViewModel
    {
        public string CourseName { get; set; }
        public string StartDate { get; set; }
        public string Errors { get; set; }
        public string DeliveryMode { get; set; }
    }

    {
        public bool Confirm { get; set; }
        public int Row { get; set; }
    }

    public class Response
    {
        public int Row { get; set; }
        public bool Confirm { get; set; }
        public string CourseName { get; set; }
        public string StartDate { get; set; }
        public string Errors { get; set; }
        public string DeliveryMode { get; set; }
    }

    public class Handler : IRequestHandler<Query, OneOf<NotFound, Response>>,
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IFileUploadProcessor _fileUploadProcessor;

        public Handler(
            IProviderContextProvider providerContextProvider,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IFileUploadProcessor fileUploadProcessor)
        {
            _providerContextProvider = providerContextProvider;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _fileUploadProcessor = fileUploadProcessor;
        }

        public async Task<OneOf<NotFound, Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var providerId = _providerContextProvider.GetProviderId();
            var courseUpload = await _sqlQueryDispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
            {
                ProviderId = providerId
            });

            if (courseUpload == null)
            {
                return new NotFound();
            }

            var venueUploadRows = await _sqlQueryDispatcher.ExecuteQuery(new GetCourseUploadRows()
            {
                CourseUploadId = courseUpload.CourseUploadId
            });

            var row = venueUploadRows.FirstOrDefault(x => x.RowNumber == request.RowNumber);
            if (row == null)
            {
                return new NotFound();
            }

            return new Response
            {
                Row = row.RowNumber,
                CourseName = row.CourseName,
                StartDate = row.StartDate,
                Errors = GetUniqueErrorMessages(row),
                DeliveryMode = row.DeliveryMode
            };
        }

        {
            var row = await _fileUploadProcessor.GetCourseUploadRowForProvider(
                _providerContextProvider.GetProviderId(), request.Row);

            if (!request.Confirm)
            {
                var validationResult = new ValidationResult(new[]
                {
                    new ValidationFailure(nameof(request.Confirm), "Confirm you want to delete this venue")
                });
                return new ModelWithErrors<Response>(new Response()
                {
                    Row = row.RowNumber,
                    CourseName = row.CourseName,
                    StartDate = row.StartDate,
                    Errors = GetUniqueErrorMessages(row),
                    DeliveryMode = row.DeliveryMode
                }, validationResult);
            }

            var deleted = await _fileUploadProcessor.DeleteCourseUploadRowForProvider(_providerContextProvider.GetProviderId(), request.Row);
            if (!deleted)
                return new NotFound();

            var (existingRows, _) = await _fileUploadProcessor.GetCourseUploadRowsForProvider(
                    _providerContextProvider.GetProviderId());
            if (existingRows.Any(x => x.Errors.Count > 0))
                return DeleteRowResult.CourseRowDeletedHasMoreErrors;
            else
                return DeleteRowResult.CourseRowDeletedHasNoMoreErrors;
        }

        private string GetUniqueErrorMessages(CourseUploadRow row)
        {
            var errors = row.Errors.Select(errorCode => Core.DataManagement.Errors.MapCourseErrorToFieldGroup(errorCode));
            return string.Join(",", errors.Distinct().ToList());
        }
    }
}
