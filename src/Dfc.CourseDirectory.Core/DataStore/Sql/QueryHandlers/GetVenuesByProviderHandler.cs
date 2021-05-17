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
            const string sql = @"
SELECT v.VenueId, p.ProviderId, v.VenueName, v.ProviderVenueRef, v.AddressLine1, v.AddressLine2, v.Town, v.County, v.Postcode,
v.Telephone, v.Email, v.Website, v.Position.Lat Latitude, v.Position.Long Longitude
FROM Pttcd.Venues v
JOIN Pttcd.Providers p ON v.ProviderUkprn = p.Ukprn
WHERE v.VenueStatus = @LiveVenueStatus
AND p.ProviderId = @ProviderId
ORDER BY v.VenueName";

            var param = new
            {
                ProviderId = query.ProviderId,
                LiveVenueStatus = VenueStatus.Live
            };

            return (await transaction.Connection.QueryAsync<Venue>(sql, param, transaction))
                .AsList();
        }
    }
}
