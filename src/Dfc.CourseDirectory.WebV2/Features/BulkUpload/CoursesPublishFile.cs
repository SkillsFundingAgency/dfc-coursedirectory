using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using MediatR;
using CosmosQueries = Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.WebV2.Features.BulkUpload
{
    public class CoursesPublishFile
    {
        public class Query : IRequest<ViewModel>
        {
            public Guid ProviderId { get; set; }
        }

        public class ViewModel
        {
            public int NumberOfCourses { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, ViewModel>
        {
            private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

            public QueryHandler(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
            {
                _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            }

            public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
            {
                var cosmosProvider = await _cosmosDbQueryDispatcher.ExecuteQuery(new CosmosQueries.GetProviderById()
                {
                    ProviderId = request.ProviderId
                });
                var results = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetAllCoursesForProvider
                {
                    ProviderUkprn = cosmosProvider.Ukprn,
                    CourseStatuses = CourseStatus.BulkUploadReadyToGoLive,
                });
                return new ViewModel {NumberOfCourses = results.SelectMany(c => c.CourseRuns).Count()};
            }
        }
    }
}
