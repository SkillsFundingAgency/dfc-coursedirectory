using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class CreateApprenticeshipUploadHandler : ISqlQueryHandler<CreateApprenticeshipUpload, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, CreateApprenticeshipUpload query)
        {
            var sql = $@"
INSERT INTO Pttcd.ApprenticeshipUploads (
    ApprenticeshipUploadId,
    ProviderId,
    UploadStatus,
    CreatedOn,
    CreatedByUserId
) VALUES (
    @ApprenticeshipUploadId,
    @ProviderId,
    {(int)UploadStatus.Created},
    @CreatedOn,
    @CreatedByUserId
)";

            var paramz = new
            {
                query.ApprenticeshipUploadId,
                query.ProviderId,
                query.CreatedOn,
                CreatedByUserId = query.CreatedBy.UserId
            };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new Success();
        }
    }
}
