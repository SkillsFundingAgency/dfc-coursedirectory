using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class DeleteVenueHandler : ISqlQueryHandler<DeleteVenue, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(SqlTransaction transaction, DeleteVenue query)
        {
            var sql = $@"
UPDATE Pttcd.Venues SET
    VenueStatus = {(int)VenueStatus.Archived},
    UpdatedOn = @DeletedOn,
    UpdatedBy = @DeletedByUserId
WHERE VenueId = @VenueId
AND VenueStatus <> {(int)VenueStatus.Archived}";

            var paramz = new
            {
                query.VenueId,
                query.DeletedOn,
                DeletedByUserId = query.DeletedBy.UserId
            };

            var deleted = (await transaction.Connection.ExecuteAsync(sql, paramz, transaction)) == 1;

            if (deleted)
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
