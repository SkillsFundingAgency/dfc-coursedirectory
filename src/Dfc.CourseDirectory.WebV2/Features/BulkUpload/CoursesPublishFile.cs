using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.Models;
using MediatR;
using CosmosQueries = Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.WebV2.Features.BulkUpload.CoursesPublishFile
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
        private readonly IProviderInfoCache _providerInfoCache;

        public QueryHandler(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher, IProviderInfoCache providerInfoCache)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher ?? throw new ArgumentNullException(nameof(cosmosDbQueryDispatcher));
            _providerInfoCache = providerInfoCache ?? throw new ArgumentNullException(nameof(providerInfoCache));
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {

            var providerInfo = await _providerInfoCache.GetProviderInfo(request.ProviderId);

            var results = await _cosmosDbQueryDispatcher.ExecuteQuery(new CosmosQueries.GetAllCoursesForProvider
            {
                ProviderUkprn = providerInfo.Ukprn,
                CourseStatuses = CourseStatus.BulkUploadReadyToGoLive,
            });

            return new ViewModel {NumberOfCourses = results.SelectMany(c => c.CourseRuns).Count()};
        }
    }
}
