using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Apprenticeships.InProgress
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

            var apprenticeshipUpload = await _sqlQueryDispatcher.ExecuteQuery(new GetLatestUnpublishedApprenticeshipUploadForProvider()
            {
                ProviderId = providerId
            });

            if (apprenticeshipUpload == null)
            {
                return new NotFound();
            }

            return apprenticeshipUpload.UploadStatus;
        }
    }
}
