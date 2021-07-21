using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using static Dapper.SqlMapper;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpsertApprenticeshipUploadRowsHandler : ISqlQueryHandler<UpsertApprenticeshipUploadRows, IReadOnlyCollection<ApprenticeshipUploadRow>>
    {
        public async Task<IReadOnlyCollection<ApprenticeshipUploadRow>> Execute(SqlTransaction transaction, UpsertApprenticeshipUploadRows query)
        {
            var sql = $@"
MERGE Pttcd.ApprenticeshipUploadRows AS target
USING (SELECT * FROM @Rows) AS source
    ON target.ApprenticeshipUploadId = @ApprenticeshipUploadId AND target.RowNumber = source.RowNumber
WHEN NOT MATCHED THEN 
    INSERT (
      ApprenticeshipUploadId,
      RowNumber,
      ApprenticeshipUploadRowStatus,
      IsValid,
      Errors,
      LastUpdated,
      LastValidated,
      StandardCode,
      StandardVersion,
      ApprenticeshipInformation,
      ApprenticeshipWebpage,
      ContactEmail,
      ContactPhone,
      ContactUrl,
      DeliveryMode,
      Venue,
      YourVenueReference,
      Radius,
      NationalDelivery,
      SubRegion
    ) VALUES (
        @ApprenticeshipUploadId,
        source.RowNumber,
        {(int)UploadRowStatus.Default},
        source.IsValid,
        source.Errors,
        ISNULL(source.LastUpdated, source.LastValidated),
        source.LastValidated,
        source.StandardCode,
        source.StandardVersion,
        source.ApprenticeshipInformation,
        source.ApprenticeshipWebpage,
        source.ContactEmail,
        source.ContactPhone,
        source.ContactUrl,
        source.DeliveryMode,
        source.Venue,
        source.YourVenueReference,
        source.Radius,
        source.NationalDelivery,
        source.SubRegion
    )
WHEN MATCHED THEN UPDATE SET
    RowNumber = source.RowNumber,
    IsValid = source.IsValid,
    Errors = source.Errors,
    LastUpdated = ISNULL(source.LastUpdated, target.LastValidated),
    LastValidated = source.LastValidated,
    StandardCode = source.StandardCode,
    StandardVersion = source.StandardVersion,
    ApprenticeshipInformation = source.ApprenticeshipInformation,
    ApprenticeshipWebpage = source.ApprenticeshipWebpage,
    ContactEmail = source.ContactEmail,
    ContactPhone = source.ContactPhone,
    ContactUrl = source.ContactUrl,
    DeliveryMode = source.DeliveryMode,
    Venue = source.Venue,
    YourVenueReference = source.YourVenueReference,
    Radius = source.Radius,
    NationalDelivery = source.NationalDelivery,
    SubRegion = source.SubRegion
;

SELECT
    RowNumber, IsValid, Errors AS ErrorList, LastUpdated, LastValidated,
    StandardCode, StandardVersion, ApprenticeshipInformation, ApprenticeshipWebpage,ContactEmail,
    ContactPhone, ContactUrl, DeliveryMode, Venue,YourVenueReference, Radius, NationalDelivery,
    SubRegion
FROM Pttcd.ApprenticeshipUploadRows
WHERE ApprenticeshipUploadId = @ApprenticeshipUploadId
AND ApprenticeshipUploadRowStatus = {(int)UploadRowStatus.Default}
ORDER BY RowNumber";

            var paramz = new
            {
                query.ApprenticeshipUploadId,
                Rows = CreateRowsTvp()
            };

            var results = (await transaction.Connection.QueryAsync<Result>(sql, paramz, transaction))
                .AsList();

            foreach (var row in results)
            {
                row.Errors = (row.ErrorList ?? string.Empty).Split(";", StringSplitOptions.RemoveEmptyEntries);
            }

            return results;

            ICustomQueryParameter CreateRowsTvp()
            {
                var table = new DataTable();
                table.Columns.Add("RowNumber", typeof(int));
                table.Columns.Add("IsValid", typeof(bool));
                table.Columns.Add("Errors", typeof(string));
                table.Columns.Add("LastUpdated", typeof(DateTime));
                table.Columns.Add("LastValidated", typeof(DateTime));
                table.Columns.Add("StandardCode", typeof(int));
                table.Columns.Add("StandardVersion", typeof(int));
                table.Columns.Add("ApprenticeshipInformation", typeof(string));
                table.Columns.Add("ApprenticeshipWebpage", typeof(string));
                table.Columns.Add("ContactEmail", typeof(string));
                table.Columns.Add("ContactPhone", typeof(string));
                table.Columns.Add("ContactUrl", typeof(string));
                table.Columns.Add("DeliveryMode", typeof(string));
                table.Columns.Add("Venue", typeof(string));
                table.Columns.Add("YourVenueReference", typeof(string));
                table.Columns.Add("Radius", typeof(string));
                table.Columns.Add("NationalDelivery", typeof(string));
                table.Columns.Add("SubRegion", typeof(string));

                foreach (var record in query.Records)
                {
                    table.Rows.Add(
                        record.RowNumber,
                        record.IsValid,
                        string.Join(";", record.Errors ?? Enumerable.Empty<string>()), // Errors
                        query.UpdatedOn,
                        query.ValidatedOn,
                        record.StandardCode,
                        record.StandardVersion,
                        record.ApprenticeshipInformation,
                        record.ApprenticeshipWebpage,
                        record.ContactEmail,
                        record.ContactPhone,
                        record.ContactUrl,
                        record.DeliveryMode,
                        record.Venue,
                        record.YourVenueReference,
                        record.Radius,
                        record.NationalDelivery,
                        record.SubRegion);
                }

                return table.AsTableValuedParameter("Pttcd.ApprenticeshipUploadRowTable");
            }
        }

        private class Result : ApprenticeshipUploadRow
        {
            public string ErrorList { get; set; }
        }
    }
}
