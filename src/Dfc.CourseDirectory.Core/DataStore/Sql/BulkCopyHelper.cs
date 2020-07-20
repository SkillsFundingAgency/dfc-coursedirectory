using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using FastMember;

namespace Dfc.CourseDirectory.Core.DataStore.Sql
{
    internal static class BulkCopyHelper
    {
        public static async Task WriteRecords<T>(IEnumerable<T> records, string tableName, SqlTransaction transaction)
        {
            var typeAccessor = TypeAccessor.Create(typeof(T));
            var columns = typeAccessor.GetMembers();

            var table = new DataTable();

            foreach (var column in columns)
            {
                var columnType = column.Type;

                if (columnType.IsGenericType && columnType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    columnType = Nullable.GetUnderlyingType(columnType);
                }

                table.Columns.Add(column.Name, columnType);
            }

            foreach (var record in records)
            {
                var itemArray = columns.Select(m => typeAccessor[record, m.Name]).ToArray();
                var row = table.Rows.Add(itemArray);
            }

            using (var bulk = new SqlBulkCopy(transaction.Connection, new SqlBulkCopyOptions(), transaction))
            {
                foreach (DataColumn c in table.Columns)
                {
                    bulk.ColumnMappings.Add(new SqlBulkCopyColumnMapping(c.ColumnName, c.ColumnName));
                }

                bulk.DestinationTableName = tableName;
                await bulk.WriteToServerAsync(table);
            }
        }
    }
}
