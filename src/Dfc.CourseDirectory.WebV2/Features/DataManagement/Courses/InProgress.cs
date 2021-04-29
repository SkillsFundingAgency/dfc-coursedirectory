using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using MediatR;
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
        private readonly IProviderContextProvider _providerContextProvider;

        public Handler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            IProviderContextProvider providerContextProvider)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _providerContextProvider = providerContextProvider;
        }

        public async Task<OneOf<NotFound, UploadStatus>> Handle(Query request, CancellationToken cancellationToken)
        {
            var providerId = _providerContextProvider.GetProviderId();

            var courseUpload = await _sqlQueryDispatcher.ExecuteQuery(new GetLatestCourseUploadForProviderWithStatus()
            {
                ProviderId = providerId,
                Statuses = new[] { UploadStatus.Created, UploadStatus.InProgress, UploadStatus.Processed }
            });

            if (courseUpload == null)
            {
                return new NotFound();
            }

            return courseUpload.UploadStatus;
        }
    }
}
