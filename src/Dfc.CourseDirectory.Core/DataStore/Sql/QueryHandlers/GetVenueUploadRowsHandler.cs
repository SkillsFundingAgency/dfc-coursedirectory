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
    public class GetVenueUploadRowsHandler : ISqlQueryHandler<GetVenueUploadRows, IReadOnlyCollection<VenueUploadRow>>
    {
        public async Task<IReadOnlyCollection<VenueUploadRow>> Execute(SqlTransaction transaction, GetVenueUploadRows query)
        {
            var sql = $@"
SELECT
    RowNumber, IsValid, Errors AS ErrorList, IsSupplementary, VenueId, LastUpdated, LastValidated,
    VenueName, ProviderVenueRef, AddressLine1, AddressLine2, Town, County, Postcode, Telephone, Email, Website
FROM Pttcd.VenueUploadRows
WHERE VenueUploadId = @VenueUploadId
AND VenueUploadRowStatus = {(int)VenueUploadRowStatus.Default}
ORDER BY RowNumber";

            var results = (await transaction.Connection.QueryAsync<Result>(sql, new { query.VenueUploadId }, transaction))
                .AsList();

            foreach (var row in results)
            {
                row.Errors = (row.ErrorList ?? string.Empty).Split(";", StringSplitOptions.RemoveEmptyEntries);
            }

            return results;
        }

        private class Result : VenueUploadRow
        {
            public string ErrorList { get; set; }
        }
    }
}
