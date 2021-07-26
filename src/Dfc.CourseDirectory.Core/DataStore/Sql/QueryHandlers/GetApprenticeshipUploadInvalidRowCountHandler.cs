using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetApprenticeshipUploadInvalidRowCountHandler : ISqlQueryHandler<GetApprenticeshipUploadInvalidRowCount, int>
    {
        public Task<int> Execute(SqlTransaction transaction, GetApprenticeshipUploadInvalidRowCount query)
        {
            var sql = $@"
SELECT COUNT(1) FROM Pttcd.ApprenticeshipUploadRows
WHERE ApprenticeshipUploadId = @ApprenticeshipUploadId
AND ApprenticeshipUploadRowStatus = {(int)UploadRowStatus.Default}
AND IsValid = 0";

            return transaction.Connection.QuerySingleAsync<int>(sql, new { query.ApprenticeshipUploadId }, transaction);
        }
    }
}
