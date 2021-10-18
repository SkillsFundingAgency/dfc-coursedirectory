using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class DeleteApprenticeshipHandler : ISqlQueryHandler<DeleteApprenticeship, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(SqlTransaction transaction, DeleteApprenticeship query)
        {
            var sql = @$"
DECLARE @Deleted INT

UPDATE Pttcd.Apprenticeships SET
    ApprenticeshipStatus = {(int)ApprenticeshipStatus.Archived},
    UpdatedOn = @DeletedOn,
    UpdatedBy = @DeletedByUserId
WHERE ApprenticeshipId = @ApprenticeshipId
AND ApprenticeshipStatus <> {(int)ApprenticeshipStatus.Archived}

SET @Deleted = @@ROWCOUNT

UPDATE Pttcd.ApprenticeshipLocations
SET ApprenticeshipLocationStatus = {(int)ApprenticeshipStatus.Archived}
WHERE ApprenticeshipId = @ApprenticeshipId
AND ApprenticeshipLocationStatus <> {(int)ApprenticeshipStatus.Archived}

SELECT @Deleted Deleted";

            var paramz = new
            {
                query.ApprenticeshipId,
                query.DeletedOn,
                DeletedByUserId = query.DeletedBy.UserId
            };

            var deleted = (await transaction.Connection.QuerySingleAsync<int>(sql, paramz, transaction)) == 1;

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
