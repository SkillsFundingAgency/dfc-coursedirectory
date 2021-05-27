using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetVenueUploadRowHandler : ISqlQueryHandler<GetVenueUploadRow, VenueUploadRow>
    {
        public Task<VenueUploadRow> Execute(SqlTransaction transaction, GetVenueUploadRow query)
        {
            var sql = $@"
SELECT
    RowNumber, IsValid, Errors AS ErrorList, IsSupplementary, OutsideOfEngland, VenueId, LastUpdated, LastValidated,
    VenueName, ProviderVenueRef, AddressLine1, AddressLine2, Town, County, Postcode, Telephone, Email, Website
FROM Pttcd.VenueUploadRows
WHERE VenueUploadRowId = @VenueUploadRowId";

            return transaction.Connection.QuerySingleOrDefaultAsync<VenueUploadRow>(sql, new { query.VenueUploadRowId }, transaction);
        }
    }
}
