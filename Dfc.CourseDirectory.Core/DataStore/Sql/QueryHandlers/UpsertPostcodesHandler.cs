using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpsertPostcodesHandler : ISqlQueryHandler<UpsertPostcodes, None>
    {
        private static readonly int _commandTimeout = (int)TimeSpan.FromMinutes(5).TotalSeconds;

        public async Task<None> Execute(SqlTransaction transaction, UpsertPostcodes query)
        {
            var table = new DataTable();
            table.Columns.Add("Postcode", typeof(string));
            table.Columns.Add("Latitude", typeof(double));
            table.Columns.Add("Longitude", typeof(double));
            table.Columns.Add("InEngland", typeof(bool));

            foreach (var record in query.Records)
            {
                table.Rows.Add(
                    record.Postcode,
                    record.Position.Latitude,
                    record.Position.Longitude,
                    record.InEngland);
            }

            var createTempTableSql = @"
DROP TABLE IF EXISTS #Postcodes

CREATE TABLE #Postcodes (
    Postcode varchar(9) COLLATE SQL_Latin1_General_CP1_CI_AS,
    Latitude float,
    Longitude float,
    InEngland bit
)";

            await transaction.Connection.ExecuteAsync(createTempTableSql, transaction: transaction);

            using (var bulk = new SqlBulkCopy(transaction.Connection, new SqlBulkCopyOptions(), transaction))
            {
                bulk.BulkCopyTimeout = _commandTimeout;
                bulk.DestinationTableName = "#Postcodes";
                await bulk.WriteToServerAsync(table);
            }

            var upsertSql = @"
MERGE Pttcd.Postcodes AS target
USING (SELECT * FROM #Postcodes) AS source
ON source.Postcode = target.Postcode
WHEN NOT MATCHED THEN
    INSERT (Postcode, Position, InEngland)
        VALUES (source.Postcode, geography::Point(source.Latitude, source.Longitude, 4326), source.InEngland)
;";

            await transaction.Connection.ExecuteAsync(upsertSql, transaction: transaction);

            return new None();
        }
    }
}
