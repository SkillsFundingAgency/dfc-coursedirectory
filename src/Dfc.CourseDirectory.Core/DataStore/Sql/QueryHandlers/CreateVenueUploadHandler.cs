using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class CreateVenueUploadHandler : ISqlQueryHandler<CreateVenueUpload, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, CreateVenueUpload query)
        {
            var sql = $@"
INSERT INTO Pttcd.VenueUploads (
    VenueUploadId,
    ProviderId,
    UploadStatus,
    CreatedOn,
    CreatedByUserId
) VALUES (
    @VenueUploadId,
    @ProviderId,
    {(int)UploadStatus.Created},
    @CreatedOn,
    @CreatedByUserId
)";

            var paramz = new
            {
                query.VenueUploadId,
                query.ProviderId,
                query.CreatedOn,
                CreatedByUserId = query.CreatedBy.UserId
            };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new Success();
        }
    }
}
