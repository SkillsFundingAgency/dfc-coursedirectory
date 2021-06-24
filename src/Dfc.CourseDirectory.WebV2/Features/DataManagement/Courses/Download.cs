using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
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

        public Handler(
            IProviderContextProvider providerContextProvider,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IClock clock)
        {
            _providerContextProvider = providerContextProvider;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _clock = clock;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var providerContext = _providerContextProvider.GetProviderContext();

            var courses = await _sqlQueryDispatcher.ExecuteQuery(new GetCoursesForProvider()
            {
                ProviderId = providerContext.ProviderInfo.ProviderId
            });

            //var courseRuns = await _sqlQueryDispatcher.ExecuteQuery(new GetCourse()
            //{
            //    ProviderId = providerContext.ProviderInfo.ProviderId
            //});

            var rows = courses
                .OrderBy(v => v.LarsQan)
                .ThenBy(v => v.CourseId)
                .Select(CsvCourseRow.FromModel)
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
