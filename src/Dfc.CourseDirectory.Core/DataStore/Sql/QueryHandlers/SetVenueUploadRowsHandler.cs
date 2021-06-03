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
using OneOf.Types;
using static Dapper.SqlMapper;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class SetVenueUploadRowsHandler : ISqlQueryHandler<SetVenueUploadRows, IReadOnlyCollection<VenueUploadRow>>
    {
        public async Task<IReadOnlyCollection<VenueUploadRow>> Execute(SqlTransaction transaction, SetVenueUploadRows query)
        {
            var sql = $@"
MERGE Pttcd.VenueUploadRows AS target
USING (SELECT * FROM @Rows) AS source
    ON target.VenueUploadId = @VenueUploadId AND target.RowNumber = source.RowNumber
WHEN NOT MATCHED BY SOURCE AND target.VenueUploadId = @VenueUploadId THEN
    UPDATE SET VenueUploadRowStatus = {(int)UploadRowStatus.Deleted}
WHEN NOT MATCHED THEN 
    INSERT (
        VenueUploadId,
        RowNumber,
        VenueUploadRowStatus,
        IsValid,
        Errors,
        LastUpdated,
        LastValidated,
        ProviderVenueRef,
        VenueName,
        AddressLine1,
        AddressLine2,
        Town,
        County,
        Postcode,
        Email,
        Telephone,
        Website,
        VenueId,
        OutsideOfEngland,
        IsSupplementary
    ) VALUES (
        @VenueUploadId,
        source.RowNumber,
        {(int)UploadRowStatus.Default},
        source.IsValid,
        source.Errors,
        ISNULL(source.LastUpdated, source.LastValidated),
        source.LastValidated,
        source.ProviderVenueRef,
        source.VenueName,
        source.AddressLine1,
        source.AddressLine2,
        source.Town,
        source.County,
        source.Postcode,
        source.Email,
        source.Telephone,
        source.Website,
        source.VenueId,
        source.OutsideOfEngland,
        source.IsSupplementary
    )
WHEN MATCHED THEN UPDATE SET
    RowNumber = source.RowNumber,
    IsValid = source.IsValid,
    Errors = source.Errors,
    LastUpdated = ISNULL(source.LastUpdated, target.LastValidated),
    LastValidated = source.LastValidated,
    ProviderVenueRef = source.ProviderVenueRef,
    VenueName = source.VenueName,
    AddressLine1 = source.AddressLine1,
    AddressLine2 = source.AddressLine2,
    Town = source.Town,
    County = source.County,
    Postcode = source.Postcode,
    Email = source.Email,
    Telephone = source.Telephone,
    Website = source.Website,
    VenueId = source.VenueId,
    OutsideOfEngland = source.OutsideOfEngland,
    IsSupplementary = source.IsSupplementary
;

SELECT
    RowNumber, IsValid, Errors AS ErrorList, IsSupplementary, OutsideOfEngland, VenueId, LastUpdated, LastValidated,
    VenueName, ProviderVenueRef, AddressLine1, AddressLine2, Town, County, Postcode, Telephone, Email, Website
FROM Pttcd.VenueUploadRows
WHERE VenueUploadId = @VenueUploadId
AND VenueUploadRowStatus = {(int)UploadRowStatus.Default}
ORDER BY RowNumber";

            var paramz = new
            {
                query.VenueUploadId,
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
                table.Columns.Add("ProviderVenueRef", typeof(string));
                table.Columns.Add("VenueName", typeof(string));
                table.Columns.Add("AddressLine1", typeof(string));
                table.Columns.Add("AddressLine2", typeof(string));
                table.Columns.Add("Town", typeof(string));
                table.Columns.Add("County", typeof(string));
                table.Columns.Add("Postcode", typeof(string));
                table.Columns.Add("Email", typeof(string));
                table.Columns.Add("Telephone", typeof(string));
                table.Columns.Add("Website", typeof(string));
                table.Columns.Add("VenueId", typeof(Guid));
                table.Columns.Add("OutsideOfEngland", typeof(bool));
                table.Columns.Add("IsSupplementary", typeof(bool));

                foreach (var record in query.Records)
                {
                    table.Rows.Add(
                        record.RowNumber,
                        record.IsValid,
                        string.Join(";", record.Errors ?? Enumerable.Empty<string>()), // Errors
                        query.UpdatedOn,
                        query.ValidatedOn,
                        record.ProviderVenueRef,
                        record.VenueName,
                        record.AddressLine1,
                        record.AddressLine2,
                        record.Town,
                        record.County,
                        record.Postcode,
                        record.Email,
                        record.Telephone,
                        record.Website,
                        record.VenueId,
                        record.OutsideOfEngland,
                        record.IsSupplementary);
                }

                return table.AsTableValuedParameter("Pttcd.VenueUploadRowTable");
            }
        }

        private class Result : VenueUploadRow
        {
            public string ErrorList { get; set; }
        }
    }
}
