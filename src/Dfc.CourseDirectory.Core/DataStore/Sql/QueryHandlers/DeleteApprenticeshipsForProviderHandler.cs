using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class DeleteApprenticeshipsForProviderHandler : ISqlQueryHandler<DeleteApprenticeshipsForProvider, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, DeleteApprenticeshipsForProvider query)
        {
            var sql = $@"
UPDATE Pttcd.ApprenticeshipLocations
SET ApprenticeshipLocationStatus = {(int)ApprenticeshipStatus.Archived}
FROM Pttcd.ApprenticeshipLocations al
JOIN Pttcd.Apprenticeships a ON al.ApprenticeshipId = a.ApprenticeshipId
WHERE al.ApprenticeshipLocationStatus <> {(int)ApprenticeshipStatus.Archived}
AND a.ProviderId = @ProviderId

UPDATE Pttcd.Apprenticeships
SET ApprenticeshipStatus = {(int)ApprenticeshipStatus.Archived}
WHERE ApprenticeshipStatus <> {(int)ApprenticeshipStatus.Archived}
AND ProviderId = @ProviderId";

            var paramz = new
            {
                query.ProviderId
            };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new Success();
        }
    }
}
