using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetVenueOfferingInfoHandler : ISqlQueryHandler<GetVenueOfferingInfo, VenueOfferingInfo>
    {
        public async Task<VenueOfferingInfo> Execute(SqlTransaction transaction, GetVenueOfferingInfo query)
        {
            var sql = $@"
SELECT v.VenueId, v.ProviderId, v.ProviderUkprn, v.VenueName, v.ProviderVenueRef, v.AddressLine1, v.AddressLine2, v.Town, v.County, v.Postcode,
v.Telephone, v.Email, v.Website, v.Position.Lat Latitude, v.Position.Long Longitude
FROM Pttcd.Venues v
WHERE v.VenueStatus = {(int)VenueStatus.Live}
AND v.VenueId = @VenueId

SELECT CourseId, CourseRunId FROM Pttcd.CourseRuns (HOLDLOCK)
WHERE VenueId = @VenueId
AND CourseRunStatus = {(int)CourseStatus.Live}

SELECT TLevelId, TLevelLocationId FROM Pttcd.TLevelLocations (HOLDLOCK)
WHERE VenueId = @VenueId
AND TLevelLocationStatus = {(int)TLevelLocationStatus.Live}

SELECT ApprenticeshipId, ApprenticeshipLocationId FROM Pttcd.ApprenticeshipLocations (HOLDLOCK)
WHERE VenueId = @VenueId
AND ApprenticeshipLocationStatus <> {(int)ApprenticeshipStatus.Archived}
";

            var param = new
            {
                VenueId = query.VenueId
            };

            using (var reader = await transaction.Connection.QueryMultipleAsync(sql, param, transaction))
            {
                var venue = await reader.ReadSingleOrDefaultAsync<Venue>();

                if (venue == null)
                {
                    return null;
                }

                var linkedCourses = (await reader.ReadAsync<(Guid, Guid)>()).AsList();
                var linkedTLevels = (await reader.ReadAsync<(Guid, Guid)>()).AsList();
                var linkedApprenticeships = (await reader.ReadAsync<(Guid, Guid)>()).AsList();

                return new VenueOfferingInfo()
                {
                    Venue = venue,
                    LinkedApprenticeships = linkedApprenticeships,
                    LinkedCourses = linkedCourses,
                    LinkedTLevels = linkedTLevels
                };
            }
        }
    }
}
