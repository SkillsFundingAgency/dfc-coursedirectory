using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetCountCoursesHandler :
        ISqlQueryHandler<GetCountCourses, IReadOnlyCollection<CourseResult>>
    {
        public async Task<IReadOnlyCollection<CourseResult>> Execute(
            SqlTransaction transaction,
            GetCountCourses query)
        {
            var sql = $@"
SELECT
	COUNT(c.CourseID) AS TotalCourses, COUNT(IIF(cr.StartDate < GETDATE() - 61,1,NULL)) AS OutofDateCourses
FROM Pttcd.Courses c
Full JOIN Pttcd.CourseRuns cr ON c.CourseId = cr.CourseId
Full JOIN Pttcd.Providers p ON c.ProviderUkprn = p.Ukprn";

            var paramz = new
            {
                query.Today
            };

            using var reader = await transaction.Connection.QueryMultipleAsync(sql, paramz, transaction);

            var results = (await reader.ReadAsync<CourseResult>()).AsList();

            return results;
        }
    }
}
