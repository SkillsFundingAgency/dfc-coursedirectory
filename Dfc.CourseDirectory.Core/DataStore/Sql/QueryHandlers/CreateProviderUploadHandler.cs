using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class CreateProviderUploadHandler : ISqlQueryHandler<CreateProviderUpload, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, CreateProviderUpload query)
        {
            var sql = $@"
                INSERT INTO Pttcd.ProviderUploads (
                    ProviderUploadId,
                    UploadStatus,
                    CreatedOn,
                    CreatedByUserId,
                    InactiveProviders,
                ) VALUES (
                    @ProviderUploadId,
                    {(int)UploadStatus.Created},
                    @CreatedOn,
                    @CreatedByUserId,
                    @InactiveProviders
                )";

            var paramz = new
            {
                query.ProviderUploadId,
                query.CreatedOn,
                CreatedByUserId = query.CreatedBy.UserId,
                query.InactiveProviders
            };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new Success();
        }
    }
}
