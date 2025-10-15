using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetProviderUploadInvalidRowCountHandler : ISqlQueryHandler<GetProviderUploadInvalidRowCount, int>
    {
        public Task<int> Execute(SqlTransaction transaction, GetProviderUploadInvalidRowCount query)
        {
            var sql = $@"
SELECT COUNT(1) FROM Pttcd.ProviderUploadRows
WHERE ProviderUploadId = @ProviderUploadId
AND ProviderUploadRowStatus = {(int)UploadRowStatus.Default}
AND IsValid = 0";

            return transaction.Connection.QuerySingleAsync<int>(sql, new { query.ProviderUploadId }, transaction);
        }
    }
}
