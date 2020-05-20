using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using FastMember;

namespace Dfc.CourseDirectory.Core.DataStore.Sql
{
    internal static class UpsertHelper
    {
        private const string Collation = "SQL_Latin1_General_CP1_CI_AS";

        private static readonly int _commandTimeout = (int)TimeSpan.FromMinutes(5).TotalSeconds;

        public static async Task Upsert<T>(
            SqlTransaction transaction,
            IEnumerable<T> records,
            IEnumerable<string> keyPropertyNames,
            string tableName,
            bool deleteIfNotInSource = false)
        {
            var tempTableName = "#UpsertHelper";

            var typeAccessor = TypeAccessor.Create(typeof(T));

            await CreateTableVariable();
            await InsertDataIntoTableVariable();
            await MergeRecords();
            await DropTempTable();

            Task CreateTableVariable()
            {
                var sqlCommandBuilder = new SqlCommandBuilder();

                var sql = new StringBuilder();

                sql.AppendLine($"CREATE TABLE {tempTableName} (");
                sql.AppendLine(string.Join(",\n", typeAccessor.GetMembers().Select(column => $"{sqlCommandBuilder.QuoteIdentifier(column.Name)} {GetSqlColumnTypeFromClrType(column.Type)}")));
                sql.AppendLine(")");

                return transaction.Connection.ExecuteAsync(sql.ToString(), transaction: transaction);

                string GetSqlColumnTypeFromClrType(Type type)
                {
                    if (type == typeof(string))
                    {
                        return $"NVARCHAR(MAX) COLLATE {Collation}";
                    }
                    else if (type == typeof(DateTime) || type == typeof(DateTime?))
                    {
                        return "DATETIME";
                    }
                    else
                    {
                        throw new NotSupportedException($"Cannot determine SQL type from '{type.Name}'.");
                    }
                }
            }

            async Task InsertDataIntoTableVariable()
            {
                var sqlCommandBuilder = new SqlCommandBuilder();

                var sql = new StringBuilder();

                sql.AppendLine($"INSERT INTO {tempTableName} (");
                sql.AppendLine(string.Join(",", columns.Select(sqlCommandBuilder.QuoteIdentifier)));
                sql.AppendLine(") VALUES (");
                sql.AppendLine(string.Join(",", columns.Select(GetParameterNameForColumn)));
                sql.AppendLine(")");

                foreach (var record in records)
                {
                    var parameters = new DynamicParameters();

                    foreach (var column in columns)
                    {
                        parameters.Add(GetParameterNameForColumn(column), typeAccessor[record, column]);
                    }

                    await transaction.Connection.ExecuteAsync(
                        sql.ToString(),
                        parameters,
                        transaction: transaction,
                        commandTimeout: _commandTimeout);
                }

                static string GetParameterNameForColumn(string columnName) => "@" + columnName;
            }

            Task MergeRecords()
            {
                var sqlCommandBuilder = new SqlCommandBuilder();

                var sql = new StringBuilder();

                sql.AppendLine($"MERGE {tableName} AS target");
                sql.AppendLine($"USING (SELECT ");
                sql.AppendLine(string.Join(", ", columns.Select(sqlCommandBuilder.QuoteIdentifier)));
                sql.AppendLine($"FROM {tempTableName}) AS source");
                sql.AppendLine($"ON");
                sql.AppendLine(string.Join(" AND ", keyPropertyNames.Select(c => $"source.{sqlCommandBuilder.QuoteIdentifier(c)} = target.{sqlCommandBuilder.QuoteIdentifier(c)}")));
                sql.AppendLine($"WHEN NOT MATCHED THEN INSERT (");
                sql.AppendLine(string.Join(", ", columns.Select(sqlCommandBuilder.QuoteIdentifier)));
                sql.AppendLine($") VALUES (");
                sql.AppendLine(string.Join(", ", columns.Select(c => $"source.{sqlCommandBuilder.QuoteIdentifier(c)}")));
                sql.AppendLine($") WHEN MATCHED THEN UPDATE SET");
                sql.AppendLine(string.Join(", ", columns.Except(keyPropertyNames).Select(c => $"{sqlCommandBuilder.QuoteIdentifier(c)} = source.{sqlCommandBuilder.QuoteIdentifier(c)}")));

                if (deleteIfNotInSource)
                {
                    sql.AppendLine("WHEN NOT MATCHED BY SOURCE THEN DELETE");
                }

                sql.AppendLine($";");

                return transaction.Connection.ExecuteAsync(
                    sql.ToString(),
                    transaction: transaction,
                    commandTimeout: _commandTimeout);
            }

            Task DropTempTable()
            {
                var sql = $"DROP TABLE {tempTableName}";

                return transaction.Connection.ExecuteAsync(sql.ToString(), transaction: transaction);
            }
        }
    }
}
