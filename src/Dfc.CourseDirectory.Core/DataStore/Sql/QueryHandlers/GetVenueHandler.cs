using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetVenueHandler : ISqlQueryHandler<GetVenue, Venue>
    {
        public Task<Venue> Execute(
            SqlTransaction transaction,
            GetVenue query)
        {
            var sql = $@"
SELECT v.VenueId, v.ProviderId, v.ProviderUkprn, v.VenueName, v.ProviderVenueRef, v.AddressLine1, v.AddressLine2, v.Town, v.County, v.Postcode,
v.Telephone, v.Email, v.Website, v.Position.Lat Latitude, v.Position.Long Longitude
FROM Pttcd.Venues v
WHERE v.VenueStatus = {(int)VenueStatus.Live}
AND v.VenueId = @VenueId";

            var param = new
            {
                VenueId = query.VenueId
            };

            return transaction.Connection.QuerySingleOrDefaultAsync<Venue>(sql, param, transaction);
        }
    }
}
