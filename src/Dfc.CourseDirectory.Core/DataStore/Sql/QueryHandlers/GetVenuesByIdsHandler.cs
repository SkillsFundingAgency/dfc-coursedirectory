using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetVenuesByIdsHandler : ISqlQueryHandler<GetVenuesByIds, IReadOnlyDictionary<Guid, Venue>>
    {
        public async Task<IReadOnlyDictionary<Guid, Venue>> Execute(
            SqlTransaction transaction,
            GetVenuesByIds query)
        {
            const string sql = @"
SELECT v.VenueId, v.VenueName, v.ProviderVenueRef, v.AddressLine1, v.AddressLine2, v.Town, v.County, v.Postcode,
v.Telephone, v.Email, v.Website
FROM Pttcd.Venues v
JOIN @VenueIds x ON v.VenueId = x.Id
WHERE v.VenueStatus = @LiveVenueStatus";

            var param = new
            {
                VenueIds = TvpHelper.CreateGuidIdTable(query.VenueIds),
                LiveVenueStatus = VenueStatus.Live
            };

            return (await transaction.Connection.QueryAsync<Venue>(sql, param, transaction))
                .ToDictionary(v => v.VenueId, v => v);
        }
    }
}
