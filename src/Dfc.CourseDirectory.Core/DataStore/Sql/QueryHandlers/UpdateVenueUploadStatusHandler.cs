using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpdateVenueUploadStatusHandler : ISqlQueryHandler<UpdateVenueUploadStatus, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(SqlTransaction transaction, UpdateVenueUploadStatus query)
        {
            var statusColumnName = query.UploadStatus switch
            {
                UploadStatus.InProgress => "ProcessingStartedOn",
                UploadStatus.Processed => "ProcessingCompletedOn",
                UploadStatus.Published => "PublishedOn",
                UploadStatus.Abandoned => "AbandonedOn",
                _ => throw new NotSupportedException($"Unknown {nameof(UploadStatus)}: '{query.UploadStatus}'.")
            };

            var sql = $@"
UPDATE Pttcd.VenueUploads
SET UploadStatus = @UploadStatus, {statusColumnName} = @ChangedOn
WHERE VenueUploadId = @VenueUploadId";

            var paramz = new
            {
                query.VenueUploadId,
                query.ChangedOn,
                query.UploadStatus
            };

            var updated = await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            if (updated == 1)
            {
                return new Success();
            }
            else
            {
                return new NotFound();
            }
        }
    }
}
