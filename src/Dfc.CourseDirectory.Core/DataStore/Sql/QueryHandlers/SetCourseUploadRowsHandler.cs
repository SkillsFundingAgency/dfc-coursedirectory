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
    public class SetCourseUploadRowsHandler : ISqlQueryHandler<SetCourseUploadRows, IReadOnlyCollection<CourseUploadRow>>
    {
        public async Task<IReadOnlyCollection<CourseUploadRow>> Execute(SqlTransaction transaction, SetCourseUploadRows query)
        {
            var sql = $@"
MERGE Pttcd.CourseUploadRows AS target
USING (SELECT * FROM @Rows) AS source
    ON target.CourseUploadId = @CourseUploadId AND target.RowNumber = source.RowNumber
WHEN NOT MATCHED BY SOURCE AND target.CourseUploadId = @CourseUploadId THEN
    UPDATE SET CourseUploadRowStatus = {(int)UploadRowStatus.Deleted}
WHEN NOT MATCHED THEN 
    INSERT (
        CourseUploadId,
        RowNumber,
        CourseUploadRowStatus,
        IsValid,
        Errors,
        LastUpdated,
        LastValidated,
        CourseId,
        CourseRunId,
        LarsQan,
        WhoThisCourseIsFor,
        EntryRequirements,
        WhatYouWillLearn,
        HowYouWillLearn,
        WhatYouWillNeedToBring,
        HowYouWillBeAssessed,
        WhereNext,
        CourseName,
        YourReference,
        DeliveryMode,
        StartDate,
        FlexibleStartDate,
        VenueName,
        ProviderVenueRef,
        NationalDelivery,
        SubRegions,
        CourseWebpage,
        Cost,
        CostDescription,
        Duration,
        DurationUnit,
        StudyMode,
        AttendancePattern
    ) VALUES (
        @CourseUploadId,
        source.RowNumber,
        {(int)UploadRowStatus.Default},
        source.IsValid,
        source.Errors,
        ISNULL(source.LastUpdated, source.LastValidated),
        source.LastValidated,
        source.CourseId,
        source.CourseRunId,
        source.LarsQan,
        source.WhoThisCourseIsFor,
        source.EntryRequirements,
        source.WhatYouWillLearn,
        source.HowYouWillLearn,
        source.WhatYouWillNeedToBring,
        source.HowYouWillBeAssessed,
        source.WhereNext,
        source.CourseName,
        source.YourReference,
        source.DeliveryMode,
        source.StartDate,
        source.FlexibleStartDate,
        source.VenueName,
        source.ProviderVenueRef,
        source.NationalDelivery,
        source.SubRegions,
        source.CourseWebpage,
        source.Cost,
        source.CostDescription,
        source.Duration,
        source.DurationUnit,
        source.StudyMode,
        source.AttendancePattern
    )
WHEN MATCHED THEN UPDATE SET
    RowNumber = source.RowNumber,
    IsValid = source.IsValid,
    Errors = source.Errors,
    LastUpdated = ISNULL(source.LastUpdated, target.LastValidated),
    LastValidated = source.LastValidated,
    CourseId = source.CourseId,
    CourseRunId = source.CourseRunId,
    LarsQan = source.LarsQan,
    WhoThisCourseIsFor = source.WhoThisCourseIsFor,
    EntryRequirements = source.EntryRequirements,
    WhatYouWillLearn = source.WhatYouWillLearn,
    HowYouWillLearn = source.HowYouWillLearn,
    WhatYouWillNeedToBring = source.WhatYouWillNeedToBring,
    HowYouWillBeAssessed = source.HowYouWillBeAssessed,
    WhereNext = source.WhereNext,
    CourseName = source.CourseName,
    YourReference = source.YourReference,
    DeliveryMode = source.DeliveryMode,
    StartDate = source.StartDate,
    FlexibleStartDate = source.FlexibleStartDate,
    VenueName = source.VenueName,
    ProviderVenueRef = source.ProviderVenueRef,
    NationalDelivery = source.NationalDelivery,
    SubRegions = source.SubRegions,
    CourseWebpage = source.CourseWebpage,
    Cost = source.Cost,
    CostDescription = source.CostDescription,
    Duration = source.Duration,
    DurationUnit = source.DurationUnit,
    StudyMode = source.StudyMode,
    AttendancePattern = source.AttendancePattern
;

SELECT
    RowNumber, IsValid, Errors AS ErrorList, CourseId, CourseRunId, LastUpdated, LastValidated,
    LarsQan, WhoThisCourseIsFor, EntryRequirements, WhatYouWillLearn, HowYouWillLearn, WhatYouWillNeedToBring,
    HowYouWillBeAssessed, WhereNext, CourseName, YourReference, DeliveryMode, StartDate, FlexibleStartDate,
    VenueName, ProviderVenueRef, NationalDelivery, SubRegions, CourseWebpage, Cost, CostDescription,
    Duration, DurationUnit, StudyMode, AttendancePattern
FROM Pttcd.CourseUploadRows
WHERE CourseUploadId = @CourseUploadId
AND CourseUploadRowStatus = {(int)UploadRowStatus.Default}
ORDER BY RowNumber";

            var paramz = new
            {
                query.CourseUploadId,
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
                table.Columns.Add("CourseId", typeof(Guid));
                table.Columns.Add("CourseRunId", typeof(Guid));
                table.Columns.Add("LarsQan", typeof(string));
                table.Columns.Add("WhoThisCourseIsFor", typeof(string));
                table.Columns.Add("EntryRequirements", typeof(string));
                table.Columns.Add("WhatYouWillLearn", typeof(string));
                table.Columns.Add("HowYouWillLearn", typeof(string));
                table.Columns.Add("WhatYouWillNeedToBring", typeof(string));
                table.Columns.Add("HowYouWillBeAssessed", typeof(string));
                table.Columns.Add("WhereNext", typeof(string));
                table.Columns.Add("CourseName", typeof(string));
                table.Columns.Add("YourReference", typeof(string));
                table.Columns.Add("DeliveryMode", typeof(string));
                table.Columns.Add("StartDate", typeof(string));
                table.Columns.Add("FlexibleStartDate", typeof(string));
                table.Columns.Add("VenueName", typeof(string));
                table.Columns.Add("ProviderVenueRef", typeof(string));
                table.Columns.Add("NationalDelivery", typeof(string));
                table.Columns.Add("SubRegions", typeof(string));
                table.Columns.Add("CourseWebpage", typeof(string));
                table.Columns.Add("Cost", typeof(string));
                table.Columns.Add("CostDescription", typeof(string));
                table.Columns.Add("Duration", typeof(string));
                table.Columns.Add("DurationUnit", typeof(string));
                table.Columns.Add("StudyMode", typeof(string));
                table.Columns.Add("AttendancePattern", typeof(string));

                foreach (var record in query.Records)
                {
                    table.Rows.Add(
                        record.RowNumber,
                        record.IsValid,
                        string.Join(";", record.Errors ?? Enumerable.Empty<string>()), // Errors
                        query.UpdatedOn,
                        query.ValidatedOn,
                        record.CourseId,
                        record.CourseRunId,
                        record.LarsQan,
                        record.WhoThisCourseIsFor,
                        record.EntryRequirements,
                        record.WhatYouWillLearn,
                        record.HowYouWillLearn,
                        record.WhatYouWillNeedToBring,
                        record.HowYouWillBeAssessed,
                        record.WhereNext,
                        record.CourseName,
                        record.YourReference,
                        record.DeliveryMode,
                        record.StartDate,
                        record.FlexibleStartDate,
                        record.VenueName,
                        record.ProviderVenueRef,
                        record.NationalDelivery,
                        record.SubRegions,
                        record.CourseWebpage,
                        record.Cost,
                        record.CostDescription,
                        record.Duration,
                        record.DurationUnit,
                        record.StudyMode,
                        record.AttendancePattern);
                }

                return table.AsTableValuedParameter("Pttcd.CourseUploadRowTable");
            }
        }

        private class Result : CourseUploadRow
        {
            public string ErrorList { get; set; }
        }
    }
}
