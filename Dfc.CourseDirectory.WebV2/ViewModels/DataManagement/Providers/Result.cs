using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Extensions;
using Dfc.CourseDirectory.Core.Middleware;
using Dfc.CourseDirectory.Core.Models;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.ViewModels.DataManagement.Providers.Result
{
    public class Query : IRequest<OneOf<NotFound, ProviderUploadResultSummary>>
    {
        public Guid ProviderUploadId { get; set; }
    }
    public class ViewModel 
    {
        public Guid ProviderUploadId { get; set; }

        public UploadStatus UploadStatus { get; set; }

        public ProviderUploadResultSummary UploadResultSummary { get; set; }

    }
    public class Handler : IRequestHandler<Query, OneOf<NotFound, ProviderUploadResultSummary>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<OneOf<NotFound, ProviderUploadResultSummary>> Handle(Query request, CancellationToken cancellationToken)
        {

            var providerUploadResult = await _sqlQueryDispatcher.ExecuteQuery(new GetProviderUploadResult()
            {
                ProviderUploadId = request.ProviderUploadId,
            });

            if (providerUploadResult == null)
            {
                return new NotFound();
            }

            return providerUploadResult;
        }
    }
}
