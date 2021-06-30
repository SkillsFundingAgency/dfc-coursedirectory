using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.Download
{
    public class Query : IRequest<Response>
    {
    }

    public class Response
    {
        public string FileName { get; set; }
        public IReadOnlyCollection<CsvCourseRow> Rows { get; set; }
    }

    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IClock _clock;
        private readonly IRegionCache _regionCache;

        public Handler(
            IProviderContextProvider providerContextProvider,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IClock clock,
            IRegionCache regionCache)
        {
            _providerContextProvider = providerContextProvider;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _clock = clock;
            _regionCache = regionCache;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var providerContext = _providerContextProvider.GetProviderContext();

            var courses = await _sqlQueryDispatcher.ExecuteQuery(new GetCoursesForProvider()
            {
                ProviderId = providerContext.ProviderInfo.ProviderId,
                CourseRunStatuses = new[] { Core.Models.CourseStatus.Live }
            });

            var allRegions = await _regionCache.GetAllRegions();
            var rows = courses
                .SelectMany(course => CsvCourseRow.FromModel(course, allRegions))
                .ToList();

            var fileName = FileNameHelper.SanitizeFileName(
                $"{providerContext.ProviderInfo.ProviderName}_courses_{_clock.UtcNow:yyyyMMddHHmm}.csv");

            return new Response()
            {
                FileName = fileName,
                Rows = rows
            };
        }
    }
}
