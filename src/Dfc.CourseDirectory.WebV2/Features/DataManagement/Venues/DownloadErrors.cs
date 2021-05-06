using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues.DownloadErrors
{
    public class Query : IRequest<Response>
    {
    }

    public class Response
    {
        public string FileName { get; set; }
        public IReadOnlyCollection<CsvVenueRowWithErrors> Rows { get; set; }
    }

    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IClock _clock;

        public Handler(
            IProviderContextProvider providerContextProvider,
            IFileUploadProcessor fileUploadProcessor,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IClock clock)
        {
            _providerContextProvider = providerContextProvider;
            _fileUploadProcessor = fileUploadProcessor;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _clock = clock;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var providerContext = _providerContextProvider.GetProviderContext();

            var venueUpload = await _sqlQueryDispatcher.ExecuteQuery(new GetLatestVenueUploadForProviderWithStatus()
            {
                ProviderId = providerContext.ProviderInfo.ProviderId,
                Statuses = new[] { UploadStatus.ProcessedWithErrors }
            });

            if (venueUpload == null)
            {
                throw new InvalidStateException();
            }

            var rows = (await _fileUploadProcessor.GetVenueUploadRows(venueUpload.VenueUploadId))
                .Select(CsvVenueRowWithErrors.FromModel)
                .ToList();

            var fileName = FileNameHelper.SanitizeFileName(
                $"{providerContext.ProviderInfo.ProviderName}_venues_errors_{_clock.UtcNow:yyyyMMddHHmm}.csv");

            return new Response()
            {
                FileName = fileName,
                Rows = rows
            };
        }
    }
}
