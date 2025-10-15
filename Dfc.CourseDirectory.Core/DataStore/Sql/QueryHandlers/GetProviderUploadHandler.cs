using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetProviderUploadHandler : ISqlQueryHandler<GetProviderUpload, ProviderUpload>
    {
        public Task<ProviderUpload> Execute(SqlTransaction transaction, GetProviderUpload query)
        {
            var sql = $@"
SELECT ProviderUploadId, UploadStatus, CreatedOn, CreatedByUserId,
ProcessingStartedOn, ProcessingCompletedOn, PublishedOn, AbandonedOn
FROM Pttcd.ProviderUploads WITH (HOLDLOCK)
WHERE ProviderUploadId = @ProviderUploadId";

            return transaction.Connection.QuerySingleOrDefaultAsync<ProviderUpload>(sql, new { query.ProviderUploadId }, transaction);
        }
    }
}
