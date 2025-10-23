using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetVenuesByProviderHandler : ISqlQueryHandler<GetVenuesByProvider, IReadOnlyCollection<Venue>>
    {
        public async Task<IReadOnlyCollection<Venue>> Execute(SqlTransaction transaction, GetVenuesByProvider query)
        {
            var sql = $@"
SELECT v.VenueId, v.ProviderId, v.ProviderUkprn, v.VenueName, v.ProviderVenueRef, v.AddressLine1, v.AddressLine2, v.Town, v.County, v.Postcode,
v.Telephone, v.Email, v.Website, v.Position.Lat Latitude, v.Position.Long Longitude
FROM Pttcd.Venues v
WHERE v.VenueStatus = {(int)VenueStatus.Live}
AND v.ProviderId = @ProviderId
ORDER BY v.VenueName";

            var param = new
            {
                ProviderId = query.ProviderId
            };

            return (await transaction.Connection.QueryAsync<Venue>(sql, param, transaction))
                .AsList();
        }
    }
}
