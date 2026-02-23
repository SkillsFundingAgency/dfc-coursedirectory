using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetProviderUploadResultHandler : ISqlQueryHandler<GetProviderUploadResult, ProviderUploadResultSummary>
    {
        public Task<ProviderUploadResultSummary> Execute(SqlTransaction transaction, GetProviderUploadResult query)
        {
            var sql = $@"Select 
  (Select count(*) from pttcd.providers where  ProviderUploadId = @ProviderUploadId) as Total,
  (Select count(*) from pttcd.providers where  uploadresult = 1 and ProviderUploadId = @ProviderUploadId) as NewProviders,
  (Select count(*) from pttcd.providers where  uploadresult = 2 and ProviderUploadId = @ProviderUploadId) as ChangeToStatus,
  (Select count(*) from pttcd.providers where  uploadresult = 3 and ProviderUploadId = @ProviderUploadId) as ChangeToType,
  (Select count(*) from pttcd.providers where  uploadresult = 4 and ProviderUploadId = @ProviderUploadId) as ChangeToStatusAndType";

            return transaction.Connection.QuerySingleOrDefaultAsync<ProviderUploadResultSummary>(sql, new { query.ProviderUploadId }, transaction);
        }
    }
}
