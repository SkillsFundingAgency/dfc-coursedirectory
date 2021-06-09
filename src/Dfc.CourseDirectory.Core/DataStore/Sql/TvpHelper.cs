using System;
using System.Collections.Generic;
using System.Data;
using Dapper;

namespace Dfc.CourseDirectory.Core.DataStore.Sql
{
    internal static class TvpHelper
    {
        public static SqlMapper.ICustomQueryParameter CreateGuidIdTable(IEnumerable<Guid> rows)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(Guid));

            foreach (var row in rows)
            {
                table.Rows.Add(row);
            }

            return table.AsTableValuedParameter(typeName: "Pttcd.GuidIdTable");
        }

        public static SqlMapper.ICustomQueryParameter CreateStringTable(IEnumerable<string> rows)
        {
            var table = new DataTable();
            table.Columns.Add("Value", typeof(string));

            foreach (var row in rows)
            {
                table.Rows.Add(row);
            }

            return table.AsTableValuedParameter(typeName: "Pttcd.StringTable");
        }

        public static SqlMapper.ICustomQueryParameter CreateUnicodeStringTable(IEnumerable<string> rows)
        {
            var table = new DataTable();
            table.Columns.Add("Value", typeof(string));

            foreach (var row in rows)
            {
                table.Rows.Add(row);
            }

            return table.AsTableValuedParameter(typeName: "Pttcd.UnicodeStringTable");
        }
    }
}
