using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.Courses.Reporting.OutofDateCoursesReport
{
    public class Query : IRequest<IAsyncEnumerable<Csv>>
    {
    }

    public class Csv
    {
        [Name("UKPRN")]
        public int ProviderUkprn { get; set; }

        [Name("Provider Name")]
        public string ProviderName { get; set; }

        [Name("Course ID")]
        public Guid CourseId { get; set; }

        [Name("Course Run ID")]
        public Guid CourseRunId { get; set; }

        [Name("Course Name")]
        public string CourseName { get; set; }

        [Name("Start Date")]
        [Format("u")]
        public DateTime StartDate { get; set; }
    }

    public class Handler : IRequestHandler<Query, IAsyncEnumerable<Csv>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public Task<IAsyncEnumerable<Csv>> Handle(Query request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Process(_sqlQueryDispatcher.ExecuteQuery(new GetOutofDateCoursesReport())));

            static async IAsyncEnumerable<Csv> Process(IAsyncEnumerable<OutofDateCourseItem> results)
            {
                await foreach (var result in results)
                {
                    yield return new Csv
                    {
                        ProviderUkprn = result.ProviderUkprn,
                        ProviderName = result.ProviderName,
                        CourseId = result.CourseId,
                        CourseRunId = result.CourseRunId,
                        CourseName = result.CourseName,
                        StartDate = result.StartDate
                    };
                }
            }
        }
    }
}
