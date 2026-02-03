using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Extensions;
using Dfc.CourseDirectory.Core.Middleware;
using Dfc.CourseDirectory.Core.Models;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.ViewModels.DataManagement.Providers.InProgress
{
    public class Query : IRequest<OneOf<NotFound, UploadStatus>>
    {
        public Guid ProviderUploadId { get; set; }
    }
    public class ViewModel 
    {
        public Guid ProviderUploadId { get; set; }

        public UploadStatus UploadStatus { get; set; }

    }
    public class Handler : IRequestHandler<Query, OneOf<NotFound, UploadStatus>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<OneOf<NotFound, UploadStatus>> Handle(Query request, CancellationToken cancellationToken)
        {

            var providerUpload = await _sqlQueryDispatcher.ExecuteQuery(new GetProviderUpload()
            {
                ProviderUploadId = request.ProviderUploadId,
            });

            if (providerUpload == null)
            {
                return new NotFound();
            }

            return providerUpload.UploadStatus;
        }
    }
}
