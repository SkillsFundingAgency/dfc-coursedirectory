using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpsertVenueUploadRowsHandler : ISqlQueryHandler<UpsertVenueUploadRows, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, UpsertVenueUploadRows query)
        {
            var table = new DataTable();
            table.Columns.Add("VenueUploadId", typeof(Guid));
            table.Columns.Add("RowNumber", typeof(int));
            table.Columns.Add("VenueUploadRowStatus", typeof(byte));
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

            foreach (var record in query.Records)
            {
                table.Rows.Add(
                    query.VenueUploadId,
                    record.RowNumber,
                    VenueUploadRowStatus.Default,
                    record.IsValid,
                    string.Join(";", record.Errors ?? Enumerable.Empty<string>()), // Errors
                    query.CreatedOn,  // LastUpdated
                    query.CreatedOn,  // LastValidated
                    record.ProviderVenueRef,
                    record.VenueName,
                    record.AddressLine1,
                    record.AddressLine2,
                    record.Town,
                    record.County,
                    record.Postcode,
                    record.Email,
                    record.Telephone,
                    record.Website);
            }

            using (var bulk = new SqlBulkCopy(transaction.Connection, SqlBulkCopyOptions.Default, transaction))
            {
                foreach (DataColumn c in table.Columns)
                {
                    bulk.ColumnMappings.Add(new SqlBulkCopyColumnMapping(c.ColumnName, c.ColumnName));
                }

                bulk.BulkCopyTimeout = 0;  // no timeout
                bulk.DestinationTableName = "Pttcd.VenueUploadRows";
                await bulk.WriteToServerAsync(table);
            }

            return new Success();
        }
    }
}
