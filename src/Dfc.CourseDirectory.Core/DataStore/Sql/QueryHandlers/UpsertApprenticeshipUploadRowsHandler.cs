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
      ApprenticeshipId,
      StandardCode,
      StandardVersion,
      ApprenticeshipInformation,
      ApprenticeshipWebpage,
      ContactEmail,
      ContactPhone,
      ContactUrl,
      DeliveryMethod,
      VenueName,
      YourVenueReference,
      Radius,
      DeliveryMode,
      NationalDelivery,
      SubRegions,
      VenueId,
      ResolvedDeliveryMethod,
      ResolvedDeliveryMode,
      ResolvedNationalDelivery,
      ResolvedRadius
    ) VALUES (
        @ApprenticeshipUploadId,
        source.RowNumber,
        {(int)UploadRowStatus.Default},
        source.IsValid,
        source.Errors,
        ISNULL(source.LastUpdated, source.LastValidated),
        source.LastValidated,
        source.ApprenticeshipId,
        source.StandardCode,
        source.StandardVersion,
        source.ApprenticeshipInformation,
        source.ApprenticeshipWebpage,
        source.ContactEmail,
        source.ContactPhone,
        source.ContactUrl,
        source.DeliveryMode,
        source.VenueName,
        source.YourVenueReference,
        source.Radius,
        source.DeliveryMode,
        source.NationalDelivery,
        source.SubRegions,
        source.VenueId,
        source.ResolvedDeliveryMethod,
        source.ResolvedDeliveryMode,
        source.ResolvedNationalDelivery,
        source.ResolvedRadius
    )
WHEN MATCHED THEN UPDATE SET
    RowNumber = source.RowNumber,
    IsValid = source.IsValid,
    Errors = source.Errors,
    LastUpdated = ISNULL(source.LastUpdated, target.LastValidated),
    LastValidated = source.LastValidated,
    ApprenticeshipId = source.ApprenticeshipId,
    StandardCode = source.StandardCode,
    StandardVersion = source.StandardVersion,
    ApprenticeshipInformation = source.ApprenticeshipInformation,
    ApprenticeshipWebpage = source.ApprenticeshipWebpage,
    ContactEmail = source.ContactEmail,
    ContactPhone = source.ContactPhone,
    ContactUrl = source.ContactUrl,
    DeliveryMethod = source.DeliveryMethod,
    VenueName = source.VenueName,
    YourVenueReference = source.YourVenueReference,
    Radius = source.Radius,
    DeliveryMode = source.DeliveryMode,
    NationalDelivery = source.NationalDelivery,
    SubRegions = source.SubRegions,
    VenueId = source.VenueId,
    ResolvedDeliveryMethod = source.ResolvedDeliveryMethod,
    ResolvedDeliveryMode =  source.ResolvedDeliveryMode,
    ResolvedNationalDelivery = source.ResolvedNationalDelivery,
    ResolvedRadius =  source.ResolvedRadius

;

MERGE Pttcd.ApprenticeshipUploadRowSubRegions AS target
USING (SELECT RowNumber, RegionId FROM @RowSubRegions) AS source
ON
    target.ApprenticeshipUploadId = @ApprenticeshipUploadId AND
    target.RowNumber = source.RowNumber AND
    target.RegionId = source.RegionId
WHEN NOT MATCHED THEN INSERT (ApprenticeshipUploadId, RowNumber, RegionId) VALUES (@ApprenticeshipUploadId, source.RowNumber, source.RegionId)
WHEN NOT MATCHED BY SOURCE AND target.ApprenticeshipUploadId = @ApprenticeshipUploadId AND target.RowNumber IN (SELECT RowNumber FROM @Rows) THEN DELETE
;


SELECT
    RowNumber, IsValid, Errors AS ErrorList, LastUpdated, LastValidated, ApprenticeshipId,
    StandardCode, StandardVersion, ApprenticeshipInformation, ApprenticeshipWebpage,ContactEmail,
    ContactPhone, ContactUrl, DeliveryMode, VenueName,YourVenueReference, Radius, DeliveryMode, NationalDelivery,
    SubRegions, VenueId, ResolvedDeliveryMethod, ResolvedDeliveryMode, ResolvedNationalDelivery, ResolvedRadius
FROM Pttcd.ApprenticeshipUploadRows
WHERE ApprenticeshipUploadId = @ApprenticeshipUploadId
AND ApprenticeshipUploadRowStatus = {(int)UploadRowStatus.Default}
ORDER BY RowNumber";

            var paramz = new
            {
                query.ApprenticeshipUploadId,
                Rows = CreateRowsTvp(),
                RowSubRegions = CreateRowSubRegionsTvp()
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
                table.Columns.Add("ApprenticeshipId", typeof(Guid));
                table.Columns.Add("StandardCode", typeof(int));
                table.Columns.Add("StandardVersion", typeof(int));
                table.Columns.Add("ApprenticeshipInformation", typeof(string));
                table.Columns.Add("ApprenticeshipWebpage", typeof(string));
                table.Columns.Add("ContactEmail", typeof(string));
                table.Columns.Add("ContactPhone", typeof(string));
                table.Columns.Add("ContactUrl", typeof(string));
                table.Columns.Add("DeliveryMethod", typeof(string));
                table.Columns.Add("VenueName", typeof(string));
                table.Columns.Add("YourVenueReference", typeof(string));
                table.Columns.Add("Radius", typeof(string));
                table.Columns.Add("DeliveryMode", typeof(string));
                table.Columns.Add("NationalDelivery", typeof(string));
                table.Columns.Add("SubRegions", typeof(string));
                table.Columns.Add("VenueId", typeof(Guid));
                table.Columns.Add("ResolvedDeliveryMethod", typeof(byte));
                table.Columns.Add("ResolvedDeliveryMode", typeof(byte));
                table.Columns.Add("ResolvedNationalDelivery", typeof(byte));
                table.Columns.Add("ResolvedRadius", typeof(int));

                foreach (var record in query.Records)
                {
                    table.Rows.Add(
                        record.RowNumber,
                        record.IsValid,
                        string.Join(";", record.Errors ?? Enumerable.Empty<string>()), // Errors
                        query.UpdatedOn,
                        query.ValidatedOn,
                        record.ApprenticeshipId,
                        record.StandardCode,
                        record.StandardVersion,
                        record.ApprenticeshipInformation,
                        record.ApprenticeshipWebpage,
                        record.ContactEmail,
                        record.ContactPhone,
                        record.ContactUrl,
                        record.DeliveryMode,
                        record.VenueName,
                        record.YourVenueReference,
                        record.Radius,
                        record.DeliveryMode,
                        record.NationalDelivery,
                        record.SubRegions,
                        record.VenueId,
                        record.ResolvedDeliveryMethod,
                        record.ResolvedDeliveryMode,
                        record.ResolvedNationalDelivery,
                        record.ResolvedRadius);
                }

                return table.AsTableValuedParameter("Pttcd.ApprenticeshipUploadRowTable");
            }

            ICustomQueryParameter CreateRowSubRegionsTvp()
            {
                var table = new DataTable();
                table.Columns.Add("RowNumber", typeof(int));
                table.Columns.Add("RegionId", typeof(string));

                var subRegionIds = query.Records
                    .SelectMany(r => (r.ResolvedSubRegions ?? Array.Empty<string>()).Select(sr => (SubRegionId: sr, RowNumber: r.RowNumber)))
                    .Where(t => t.SubRegionId != null);

                foreach (var record in subRegionIds)
                {
                    table.Rows.Add(
                        record.RowNumber,
                        record.SubRegionId);
                }

                return table.AsTableValuedParameter("Pttcd.ApprenticeshipUploadRowSubRegionsTable");
            }
        }

        private class Result : ApprenticeshipUploadRow
        {
            public string ErrorList { get; set; }
        }
    }
}
