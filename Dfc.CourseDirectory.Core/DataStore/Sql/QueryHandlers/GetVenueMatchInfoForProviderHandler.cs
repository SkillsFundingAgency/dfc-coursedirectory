using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetVenueMatchInfoForProviderHandler : ISqlQueryHandler<GetVenueMatchInfoForProvider, IReadOnlyCollection<VenueMatchInfo>>
    {
        public async Task<IReadOnlyCollection<VenueMatchInfo>> Execute(SqlTransaction transaction, GetVenueMatchInfoForProvider query)
        {
            var sql = $@"
SELECT v.VenueId, v.ProviderId, v.ProviderUkprn, v.VenueName, v.ProviderVenueRef, v.AddressLine1, v.AddressLine2, v.Town, v.County, v.Postcode,
v.Telephone, v.Email, v.Website, v.Position.Lat Latitude, v.Position.Long Longitude,
v.ProviderUkprn,
CASE WHEN c.VenueId IS NOT NULL OR t.VenueId IS NOT NULL THEN 1 ELSE 0 END AS HasLiveOfferings
FROM Pttcd.Venues v (HOLDLOCK)
LEFT JOIN (
	SELECT VenueId FROM Pttcd.CourseRuns (HOLDLOCK)
	WHERE CourseRunStatus <> {(int)CourseStatus.Archived}
	GROUP BY VenueId
) c ON c.VenueId = v.VenueId
LEFT JOIN (
	SELECT VenueId FROM Pttcd.TLevelLocations (HOLDLOCK)
	WHERE TLevelLocationStatus = {(int)TLevelLocationStatus.Live}
	GROUP BY VenueId
) t ON t.VenueId = v.VenueId
WHERE v.VenueStatus = {(int)VenueStatus.Live}
AND v.ProviderId = @ProviderId";

            var param = new
            {
                ProviderId = query.ProviderId
            };

            return (await transaction.Connection.QueryAsync<VenueMatchInfo>(sql, param, transaction))
                .AsList();
        }
    }
}
