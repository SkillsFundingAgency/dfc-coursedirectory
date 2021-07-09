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
    public class GetVenueUploadRowsHandler :
        ISqlQueryHandler<GetVenueUploadRows, (IReadOnlyCollection<VenueUploadRow> Rows, int LastRowNumber)>
    {
        public async Task<(IReadOnlyCollection<VenueUploadRow> Rows, int LastRowNumber)> Execute(
            SqlTransaction transaction,
            GetVenueUploadRows query)
        {
            var sql = $@"
SELECT
    RowNumber, IsValid, Errors AS ErrorList, IsSupplementary, OutsideOfEngland, VenueId, IsDeletable, LastUpdated, LastValidated,
    VenueName, ProviderVenueRef, AddressLine1, AddressLine2, Town, County, Postcode, Telephone, Email, Website,
    VenueUploadRowStatus
FROM Pttcd.VenueUploadRows
WHERE VenueUploadId = @VenueUploadId
AND VenueUploadRowStatus <> {(int)UploadRowStatus.Deleted}
ORDER BY RowNumber";

            var results = (await transaction.Connection.QueryAsync<Result>(sql, new { query.VenueUploadId }, transaction))
                .AsList();

            var lastRowNumber = results.Last()?.RowNumber ?? 0;

            foreach (var row in results)
            {
                row.Errors = (row.ErrorList ?? string.Empty).Split(";", StringSplitOptions.RemoveEmptyEntries);
            }

            return (results, lastRowNumber);
        }

        private class Result : VenueUploadRow
        {
            public string ErrorList { get; set; }
            public UploadRowStatus VenueUploadRowStatus { get; set; }
        }
    }
}
