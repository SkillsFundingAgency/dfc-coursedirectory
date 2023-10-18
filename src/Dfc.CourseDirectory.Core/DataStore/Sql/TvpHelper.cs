using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql
{
    internal static class TvpHelper
    {
        public static SqlMapper.ICustomQueryParameter CreateApprenticeshipLocationSubRegionsTable(
            IEnumerable<(Guid ApprenticeshipLocationId, string SubRegionId)> rows)
        {
            var table = new DataTable();
            table.Columns.Add("ApprenticeshipLocationId", typeof(Guid));
            table.Columns.Add("RegionId", typeof(string));

            foreach (var row in rows)
            {
                table.Rows.Add(new object[] { row.ApprenticeshipLocationId, row.SubRegionId });
            }

            return table.AsTableValuedParameter(typeName: "Pttcd.ApprenticeshipLocationSubRegionsTable");
        }

        public static SqlMapper.ICustomQueryParameter CreateCourseRunsTable(
            IEnumerable<(
                Guid CourseRunId,
                string CourseName,
                Guid? VenueId,
                string ProviderCourseId,
                CourseDeliveryMode DeliveryMode,
                bool FlexibleStartDate,
                DateTime? StartDate,
                string CourseWebsite,
                decimal? Cost,
                string CostDescription,
                CourseDurationUnit DurationUnit,
                int DurationValue,
                CourseStudyMode? StudyMode,
                CourseAttendancePattern? AttendancePattern,
                bool? National)> rows)
        {
            var table = new DataTable();
            table.Columns.Add("CourseRunId", typeof(Guid));
            table.Columns.Add("CourseName", typeof(string));
            table.Columns.Add("VenueId", typeof(Guid));
            table.Columns.Add("ProviderCourseId", typeof(string));
            table.Columns.Add("DeliveryMode", typeof(byte));
            table.Columns.Add("FlexibleStartDate", typeof(bool));
            table.Columns.Add("StartDate", typeof(DateTime));
            table.Columns.Add("CourseWebsite", typeof(string));
            table.Columns.Add("Cost", typeof(decimal));
            table.Columns.Add("CostDescription", typeof(string));
            table.Columns.Add("DurationUnit", typeof(byte));
            table.Columns.Add("DurationValue", typeof(int));
            table.Columns.Add("StudyMode", typeof(byte));
            table.Columns.Add("AttendancePattern", typeof(byte));
            table.Columns.Add("National", typeof(bool));

            foreach (var row in rows)
            {
                table.Rows.Add(new object[]
                {
                    row.CourseRunId,
                    row.CourseName,
                    row.VenueId,
                    row.ProviderCourseId,
                    row.DeliveryMode,
                    row.FlexibleStartDate,
                    row.StartDate,
                    row.CourseWebsite,
                    row.Cost,
                    row.CostDescription,
                    row.DurationUnit,
                    row.DurationValue,
                    row.StudyMode,
                    row.AttendancePattern,
                    row.National
                });
            }

            return table.AsTableValuedParameter(typeName: "Pttcd.CourseRunsTable");
        }

        public static SqlMapper.ICustomQueryParameter CreateCourseRunSubRegionsTable(IEnumerable<(Guid CourseRunId, string SubRegionId)> rows)
        {
            var table = new DataTable();
            table.Columns.Add("CourseRunId", typeof(Guid));
            table.Columns.Add("RegionId", typeof(string));

            foreach (var row in rows)
            {
                table.Rows.Add(new object[] { row.CourseRunId, row.SubRegionId });
            }

            return table.AsTableValuedParameter(typeName: "Pttcd.CourseRunSubRegionsTable");
        }

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
