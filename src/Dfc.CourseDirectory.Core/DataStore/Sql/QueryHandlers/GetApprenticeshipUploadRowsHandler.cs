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
    public class GetApprenticeshipUploadRowsHandler :
        ISqlQueryHandler<GetApprenticeshipUploadRows, (IReadOnlyCollection<ApprenticeshipUploadRow> ErrorRows, int TotalRows)>
    {
        public async Task<(IReadOnlyCollection<ApprenticeshipUploadRow> ErrorRows, int TotalRows)> Execute(
            SqlTransaction transaction,
            GetApprenticeshipUploadRows query)
        {
            var sql = $@"
SELECT
      RowNumber, ApprenticeshipUploadRowStatus,IsValid,Errors AS ErrorList,LastUpdated,LastValidated,ApprenticeshipId,
      StandardCode,StandardVersion,ApprenticeshipInformation,ApprenticeshipWebpage,ContactEmail,ContactPhone, ContactUrl,
      DeliveryMethod, VenueName, YourVenueReference, Radius, DeliveryModes, NationalDelivery, SubRegions, VenueId
FROM Pttcd.ApprenticeshipUploadRows
WHERE ApprenticeshipUploadId = @ApprenticeshipUploadId
AND ApprenticeshipUploadRowStatus = {(int)UploadRowStatus.Default}
{(query.WithErrorsOnly ? "AND IsValid = 0" : "")}
ORDER BY RowNumber";

            if (query.WithErrorsOnly)
            {
                sql += $@"
SELECT COUNT(*)
FROM Pttcd.ApprenticeshipUploadRows
WHERE ApprenticeshipUploadId = @ApprenticeshipUploadId
AND ApprenticeshipUploadRowStatus = {(int)UploadRowStatus.Default}";
            }

            using (var reader = await transaction.Connection.QueryMultipleAsync(sql, new { query.ApprenticeshipUploadId }, transaction))
            {
                var rows = (await reader.ReadAsync<Result>()).AsList();
                foreach (var row in rows)
                {
                    row.Errors = (row.ErrorList ?? string.Empty).Split(";", StringSplitOptions.RemoveEmptyEntries);
                }

                var totalRows = query.WithErrorsOnly ? await reader.ReadSingleAsync<int>() : rows.Count;

                return (rows, totalRows);
            }
        }

        private class Result : ApprenticeshipUploadRow
        {
            public string ErrorList { get; set; }
            public UploadRowStatus ApprenticeshipUploadRowStatus { get; set; }
        }
    }
}
