using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Middleware;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.ViewModels.DataManagement.Courses.Download
{
    public class Query : IRequest<Response>
    {
        public bool IsNonLars { get; set; }
    }

    public class Response
    {
        public string FileName { get; set; }
        public IReadOnlyCollection<CsvCourseRow> Rows { get; set; }
        public IReadOnlyCollection<CsvNonLarsCourseRow> NonLarsRows { get; set; }
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
            var allRegions = await _regionCache.GetAllRegions();

            var fileName = FileNameHelper.SanitizeFileName(
                    $"{providerContext.ProviderInfo.ProviderName}_courses_{_clock.UtcNow:yyyyMMddHHmm}.csv");

            if (request.IsNonLars)
            {
                fileName = FileNameHelper.SanitizeFileName(
                    $"{providerContext.ProviderInfo.ProviderName}_non_lars_courses_{_clock.UtcNow:yyyyMMddHHmm}.csv");
                var nonLarscourses = await _sqlQueryDispatcher.ExecuteQuery(new GetNonLarsCoursesForProvider()
                {
                    ProviderId = providerContext.ProviderInfo.ProviderId
                });

                var sectors = (await _sqlQueryDispatcher.ExecuteQuery(new GetSectors())).ToList();

                var nonLarsrows = nonLarscourses.OrderBy(x => x.LearnAimRef)
                    .ThenBy(x => x.CourseId)
                    .SelectMany(course => CsvNonLarsCourseRow.FromModel(course, sectors, allRegions))
                    .ToList();

                return new Response()
                {
                    FileName = fileName,
                    NonLarsRows = nonLarsrows
                };
            }
            else
            {
                var courses = await _sqlQueryDispatcher.ExecuteQuery(new GetCoursesForProvider()
                {
                    ProviderId = providerContext.ProviderInfo.ProviderId
                });

                var rows = courses.OrderBy(x => x.LearnAimRef)
                    .ThenBy(x => x.CourseId)
                    .SelectMany(course => CsvCourseRow.FromModel(course, allRegions))
                    .ToList();

                return new Response()
                {
                    FileName = fileName,
                    Rows = rows
                };
            }
        }
    }
}
