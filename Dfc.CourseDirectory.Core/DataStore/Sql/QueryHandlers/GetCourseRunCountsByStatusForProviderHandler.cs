using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetCourseRunCountsByStatusForProviderHandler :
        ISqlQueryHandler<GetCourseRunCountsByStatusForProvider, IReadOnlyDictionary<CourseStatus, int>>
    {
        public async Task<IReadOnlyDictionary<CourseStatus, int>> Execute(
            SqlTransaction transaction,
            GetCourseRunCountsByStatusForProvider query)
        {
            var sql = $@"
SELECT cr.CourseRunStatus Status, COUNT(*) Count
FROM Pttcd.CourseRuns cr
JOIN Pttcd.Courses c ON cr.CourseId = c.CourseId
WHERE c.ProviderId = @ProviderId
AND cr.CourseRunStatus <> ${(int)CourseStatus.Archived}";

            var paramz = new { query.ProviderId };

            return (await transaction.Connection.QueryAsync<Result>(sql, paramz, transaction))
                .ToDictionary(r => r.Status, r => r.Count);
        }

        private class Result
        {
            public CourseStatus Status { get; set; }
            public int Count { get; set; }
        }
    }
}
