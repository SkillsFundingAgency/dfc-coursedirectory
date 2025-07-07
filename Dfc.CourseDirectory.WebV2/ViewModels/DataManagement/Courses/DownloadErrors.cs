using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Middleware;
using Dfc.CourseDirectory.Core.Models;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.ViewModels.DataManagement.Courses.DownloadErrors
{
    public class Query : IRequest<Response>
    {
        public bool IsNonLars { get; set; }
    }

    public class Response
    {
        public string FileName { get; set; }
        public IReadOnlyCollection<CsvCourseRowWithErrors> Rows { get; set; }
        public IReadOnlyCollection<CsvNonLarsCourseRowWithErrors> NonLarsRows { get; set; }
    }

    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly IClock _clock;

        public Handler(
            IProviderContextProvider providerContextProvider,
            IFileUploadProcessor fileUploadProcessor,
            IClock clock)
        {
            _providerContextProvider = providerContextProvider;
            _fileUploadProcessor = fileUploadProcessor;
            _clock = clock;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var providerContext = _providerContextProvider.GetProviderContext();

            var (uploadRows, uploadStatus) = await _fileUploadProcessor.GetCourseUploadRowsForProvider(providerContext.ProviderInfo.ProviderId, request.IsNonLars);

            if (uploadStatus != UploadStatus.ProcessedWithErrors)
            {
                throw new InvalidUploadStatusException(uploadStatus, UploadStatus.ProcessedWithErrors);
            }
            if (request.IsNonLars)
            {
                var rows = uploadRows
                .Select(CsvNonLarsCourseRowWithErrors.FromModel)
                .ToList();

                var fileName = FileNameHelper.SanitizeFileName(
                    $"{providerContext.ProviderInfo.ProviderName}_nonlars_courses_errors_{_clock.UtcNow:yyyyMMddHHmm}.csv");

                return new Response()
                {
                    FileName = fileName,
                    NonLarsRows = rows
                };
            }
            else
            {
                var rows = uploadRows
                .Select(CsvCourseRowWithErrors.FromModel)
                .ToList();

                var fileName = FileNameHelper.SanitizeFileName(
                    $"{providerContext.ProviderInfo.ProviderName}_courses_errors_{_clock.UtcNow:yyyyMMddHHmm}.csv");

                return new Response()
                {
                    FileName = fileName,
                    Rows = rows
                };
            }

        }
    }
}
