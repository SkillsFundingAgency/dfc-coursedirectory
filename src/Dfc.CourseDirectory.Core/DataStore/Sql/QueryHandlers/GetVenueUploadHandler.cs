using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetVenueUploadHandler : ISqlQueryHandler<GetVenueUpload, VenueUpload>
    {
        public Task<VenueUpload> Execute(SqlTransaction transaction, GetVenueUpload query)
        {
            var sql = $@"
SELECT VenueUploadId, ProviderId, UploadStatus, CreatedOn, CreatedByUserId,
ProcessingStartedOn, ProcessingCompletedOn, PublishedOn, AbandonedOn, LastValidated
FROM Pttcd.VenueUploads
WHERE VenueUploadId = @VenueUploadId";

            return transaction.Connection.QuerySingleOrDefaultAsync<VenueUpload>(sql, new { query.VenueUploadId }, transaction);
        }
    }
}
