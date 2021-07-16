using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLiveCourseRunCountForProviderHandler : ISqlQueryHandler<GetLiveCourseRunCountForProvider, int>
    {
        public Task<int> Execute(SqlTransaction transaction, GetLiveCourseRunCountForProvider query)
        {
            var sql = $@"
SELECT COUNT(*) FROM Pttcd.Providers p
JOIN Pttcd.Courses c ON p.Ukprn = c.ProviderUkprn
JOIN Pttcd.CourseRuns cr ON c.CourseId = cr.CourseId
WHERE p.ProviderId = @ProviderId AND cr.CourseRunStatus = {(int)CourseStatus.Live}";

            var paramz = new { query.ProviderId };

            return transaction.Connection.QuerySingleAsync<int>(sql, paramz, transaction);
        }
    }
}
