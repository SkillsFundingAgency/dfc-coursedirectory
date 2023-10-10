using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.InProgress
{
    public class Query : IRequest<OneOf<NotFound, UploadStatus>>
    {
    }

    public class Handler : IRequestHandler<Query, OneOf<NotFound, UploadStatus>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ILogger<Handler> _log;
        private readonly IProviderContextProvider _providerContextProvider;

        public Handler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            ILogger<Handler> log,
            IProviderContextProvider providerContextProvider)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _log = log;
            _providerContextProvider = providerContextProvider;
        }

        public async Task<OneOf<NotFound, UploadStatus>> Handle(Query request, CancellationToken cancellationToken)
        {
            var providerId = _providerContextProvider.GetProviderId();
            _log.LogInformation($"Getting status of latest Course Upload for the provider: [{providerId}]");
            var courseUpload = await _sqlQueryDispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
            {
                ProviderId = providerId
            });

            if (courseUpload == null)
            {
                _log.LogWarning($"No Course Upload found for provider: [{providerId}].");
                return new NotFound();
            }
            _log.LogInformation($"Course Upload status [{courseUpload.UploadStatus}] for the provider: [{providerId}]");
            return courseUpload.UploadStatus;
        }
    }
}
