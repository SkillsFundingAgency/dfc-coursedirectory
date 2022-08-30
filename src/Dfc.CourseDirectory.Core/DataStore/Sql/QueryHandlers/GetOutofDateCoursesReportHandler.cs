using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    class GetOutofDateCoursesReportHandler : ISqlAsyncEnumerableQueryHandler<GetOutofDateCoursesReport,
        OutofDateCourseItem>
    {
        public async IAsyncEnumerable<OutofDateCourseItem> Execute(SqlTransaction transaction,
            GetOutofDateCoursesReport query)
        {
            var sql = @$"SELECT c.ProviderUkprn,
        p.ProviderName,
        c.CourseId,
        cr.CourseRunId,
        cr.CourseName,
        cr.StartDate
        FROM Pttcd.Courses c
        Full JOIN Pttcd.Providers p with (nolock) ON p.ProviderId = c.ProviderId
        Full JOIN Pttcd.CourseRuns cr with (nolock) ON cr.CourseId = c.CourseId
        WHERE cr.StartDate < GETDATE() - 61";
            using (var reader = await transaction.Connection.ExecuteReaderAsync(sql, transaction: transaction))
            {
                var parser = reader.GetRowParser<OutofDateCourseItem>();
                while (await reader.ReadAsync())
                {
                    yield return parser(reader);
                }
            }
        }
    }
}
