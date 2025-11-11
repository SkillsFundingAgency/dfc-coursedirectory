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
    public class UpsertProviderUploadRowsHandler : ISqlQueryHandler<UpsertProviderUploadRows, IReadOnlyCollection<ProviderUploadRow>>
    {
        public async Task<IReadOnlyCollection<ProviderUploadRow>> Execute(SqlTransaction transaction, UpsertProviderUploadRows query)
        {
            var sql = $@"
MERGE Pttcd.ProviderUploadRows AS target
USING (SELECT * FROM @Rows) AS source
    ON target.ProviderUploadId = @ProviderUploadId AND target.RowNumber = source.RowNumber
WHEN NOT MATCHED THEN 
    INSERT (
        ProviderUploadId,
        RowNumber,
        ProviderUploadRowStatus,
        IsValid,
        Errors,
        LastUpdated,
        LastValidated,
        ProviderId,
        Ukprn,
        ProviderStatus,
        ProviderType,
        ProviderName,
        TradingName
    ) VALUES (
        @ProviderUploadId,
        source.RowNumber,
        {(int)UploadRowStatus.Default},
        source.IsValid,
        source.Errors,
        ISNULL(source.LastUpdated, source.LastValidated),
        source.LastValidated,
        source.ProviderId,
        source.Ukprn,
        source.ProviderStatus,
        source.ProviderType,
        source.ProviderName,
        source.TradingName
    )
WHEN MATCHED THEN UPDATE SET
    RowNumber = source.RowNumber,
    IsValid = source.IsValid,
    Errors = source.Errors,
    LastUpdated = ISNULL(source.LastUpdated, target.LastValidated),
    LastValidated = source.LastValidated,
    ProviderId = source.ProviderId,
    Ukprn = source.Ukprn,
    ProviderStatus = source.ProviderStatus,
    ProviderType = source.ProviderType,
    ProviderName = source.ProviderName,
    TradingName = source.TradingName;

SELECT
    RowNumber, IsValid, Errors AS ErrorList, ProviderId, Ukprn, ProviderStatus, ProviderType,
    ProviderName, TradingName
FROM Pttcd.ProviderUploadRows
WHERE ProviderUploadId = @ProviderUploadId
AND ProviderUploadRowStatus = {(int)UploadRowStatus.Default}
ORDER BY RowNumber";

            var paramz = new
            {
                query.ProviderUploadId,
                Rows = CreateRowsTvpe(),
            };

            var results = (await transaction.Connection.QueryAsync<Result>(sql, paramz, transaction))
                .AsList();

            foreach (var row in results)
            {
                row.Errors = (row.ErrorList ?? string.Empty).Split(";", StringSplitOptions.RemoveEmptyEntries);
            }

            return results;

            ICustomQueryParameter CreateRowsTvpe()
            {
                var table = new DataTable();
                table.Columns.Add("RowNumber", typeof(int));
                table.Columns.Add("IsValid", typeof(bool));
                table.Columns.Add("Errors", typeof(string));
                table.Columns.Add("LastUpdated", typeof(DateTime));
                table.Columns.Add("LastValidated", typeof(DateTime));
                table.Columns.Add("ProviderId", typeof(Guid));
                table.Columns.Add("Ukprn", typeof(int));
                table.Columns.Add("ProviderStatus", typeof(int));
                table.Columns.Add("ProviderType", typeof(int));
                table.Columns.Add("ProviderName", typeof(string));
                table.Columns.Add("TradingName", typeof(string));
                table.Columns.Add("PIMSOrgStatus", typeof(string));
                table.Columns.Add("PIMSOrgStatusDate", typeof(DateTime));

                foreach (var record in query.Records)
                {
                    table.Rows.Add(
                        record.RowNumber,
                        record.IsValid,
                        string.Join(";", record.Errors ?? Enumerable.Empty<string>()), // Errors
                        query.UpdatedOn,
                        query.ValidatedOn,
                        record.ProviderId,
                        record.Ukprn,
                        record.ProviderStatus,
                        record.ProviderType,
                        record.ProviderName,
                        record.TradingName,
                        null,
                        null
                        );
                }

                return table.AsTableValuedParameter("Pttcd.ProviderUploadRowTable");
            }

        }

        private class Result : ProviderUploadRow
        {
            public string ErrorList { get; set; }
        }
    }
}
