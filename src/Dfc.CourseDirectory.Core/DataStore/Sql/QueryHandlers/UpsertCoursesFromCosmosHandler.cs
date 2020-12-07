using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpsertCoursesFromCosmosHandler : ISqlQueryHandler<UpsertCoursesFromCosmos, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, UpsertCoursesFromCosmos query)
        {
            await UpsertCourses();
            await UpsertCourseRuns();
            await UpsertCourseRunRegions();
            await UpsertCourseRunSubRegions();

            async Task UpsertCourses()
            {
                var createTableSql = @"
CREATE TABLE #Courses (
    CourseId UNIQUEIDENTIFIER,
    CourseStatus INT,
    CreatedOn DATETIME,
    CreatedBy NVARCHAR(MAX),
    UpdatedOn DATETIME,
    UpdatedBy NVARCHAR(MAX),
    TribalCourseId INT,
    LearnAimRef VARCHAR(50),
    ProviderUkprn INT,
    CourseDescription NVARCHAR(MAX),
    EntryRequirements NVARCHAR(MAX),
    WhatYoullLearn NVARCHAR(MAX),
    HowYoullLearn NVARCHAR(MAX),
    WhatYoullNeed NVARCHAR(MAX),
    HowYoullBeAssessed NVARCHAR(MAX),
    WhereNext NVARCHAR(MAX)
)";

                await transaction.Connection.ExecuteAsync(createTableSql, transaction: transaction);

                await BulkCopyHelper.WriteRecords(
                    query.Records.Select(r => new
                    {
                        r.CourseId,
                        r.CourseStatus,
                        r.CreatedOn,
                        r.CreatedBy,
                        r.UpdatedOn,
                        r.UpdatedBy,
                        r.TribalCourseId,
                        r.LearnAimRef,
                        r.ProviderUkprn,
                        r.CourseDescription,
                        r.EntryRequirements,
                        r.WhatYoullLearn,
                        r.HowYoullLearn,
                        r.WhatYoullNeed,
                        r.HowYoullBeAssessed,
                        r.WhereNext
                    }),
                    tableName: "#Courses",
                    transaction);

                var mergeSql = @"
MERGE Pttcd.Courses AS target
USING (
    SELECT
        CourseId,
        CourseStatus,
        CreatedOn,
        CreatedBy,
        UpdatedOn,
        UpdatedBy,
        TribalCourseId,
        LearnAimRef,
        ProviderUkprn,
        CourseDescription,
        EntryRequirements,
        WhatYoullLearn,
        HowYoullLearn,
        WhatYoullNeed,
        HowYoullBeAssessed,
        WhereNext
    FROM #Courses
) AS source
ON target.CourseId = source.CourseId
WHEN NOT MATCHED THEN
    INSERT (
        CourseId,
        LastSyncedFromCosmos,
        CourseStatus,
        CreatedOn,
        CreatedBy,
        UpdatedOn,
        UpdatedBy,
        TribalCourseId,
        LearnAimRef,
        ProviderUkprn,
        CourseDescription,
        EntryRequirements,
        WhatYoullLearn,
        HowYoullLearn,
        WhatYoullNeed,
        HowYoullBeAssessed,
        WhereNext
    ) VALUES (
        source.CourseId,
        @LastSyncedFromCosmos,
        source.CourseStatus,
        source.CreatedOn,
        source.CreatedBy,
        source.UpdatedOn,
        source.UpdatedBy,
        source.TribalCourseId,
        source.LearnAimRef,
        source.ProviderUkprn,
        source.CourseDescription,
        source.EntryRequirements,
        source.WhatYoullLearn,
        source.HowYoullLearn,
        source.WhatYoullNeed,
        source.HowYoullBeAssessed,
        source.WhereNext
    )
WHEN MATCHED THEN
    UPDATE SET
        CourseStatus = source.CourseStatus,
        LastSyncedFromCosmos = @LastSyncedFromCosmos,
        CreatedOn = source.CreatedOn,
        CreatedBy = source.CreatedBy,
        UpdatedOn = source.UpdatedOn,
        UpdatedBy = source.UpdatedBy,
        TribalCourseId = source.TribalCourseId,
        LearnAimRef = source.LearnAimRef,
        ProviderUkprn = source.ProviderUkprn,
        CourseDescription = source.CourseDescription,
        EntryRequirements = source.EntryRequirements,
        WhatYoullLearn = source.WhatYoullLearn,
        HowYoullLearn = source.HowYoullLearn,
        WhatYoullNeed = source.WhatYoullNeed,
        HowYoullBeAssessed = source.HowYoullBeAssessed,
        WhereNext = source.WhereNext;";

                await transaction.Connection.ExecuteAsync(
                    mergeSql,
                    param: new { query.LastSyncedFromCosmos },
                    transaction: transaction);
            }

            async Task UpsertCourseRuns()
            {
                var createTableSql = @"
CREATE TABLE #CourseRuns (
    CourseRunId UNIQUEIDENTIFIER,
    CourseId UNIQUEIDENTIFIER,
    CourseRunStatus INT,
    CreatedOn DATETIME,
    CreatedBy NVARCHAR(MAX),
    UpdatedOn DATETIME,
    UpdatedBy NVARCHAR(MAX),
    CourseName NVARCHAR(MAX),
    VenueId UNIQUEIDENTIFIER,
    ProviderCourseId NVARCHAR(MAX),
    DeliveryMode TINYINT,
    FlexibleStartDate BIT,
    StartDate DATE,
    CourseWebsite NVARCHAR(MAX),
    Cost INT,
    CostDescription NVARCHAR(MAX),
    DurationUnit TINYINT,
    DurationValue INT,
    StudyMode TINYINT,
    AttendancePattern TINYINT,
    [National] BIT
)";

                await transaction.Connection.ExecuteAsync(createTableSql, transaction: transaction);

                await BulkCopyHelper.WriteRecords(
                    query.Records.SelectMany(c => c.CourseRuns.Select(cr => new
                    {
                        cr.CourseRunId,
                        c.CourseId,
                        cr.CourseRunStatus,
                        cr.CreatedOn,
                        cr.CreatedBy,
                        cr.UpdatedOn,
                        cr.UpdatedBy,
                        cr.CourseName,
                        cr.VenueId,
                        cr.ProviderCourseId,
                        DeliveryMode = (byte)cr.DeliveryMode,
                        cr.FlexibleStartDate,
                        cr.StartDate,
                        cr.CourseWebsite,
                        Cost = cr.Cost.HasValue ? (int?)(cr.Cost * 100) : null,  // SqlBulkCopy truncates decimals - convert to int
                        cr.CostDescription,
                        DurationUnit = (byte)cr.DurationUnit,
                        cr.DurationValue,
                        cr.StudyMode,
                        AttendancePattern = (byte)cr.AttendancePattern,
                        cr.National
                    })),
                    tableName: "#CourseRuns",
                    transaction);

                var mergeSql = @"
MERGE Pttcd.CourseRuns AS target
USING (
    SELECT
        CourseRunId,
        CourseId,
        CourseRunStatus,
        CreatedOn,
        CreatedBy,
        UpdatedOn,
        UpdatedBy,
        CourseName,
        VenueId,
        ProviderCourseId,
        DeliveryMode,
        FlexibleStartDate,
        StartDate,
        CourseWebsite,
        Cost,
        CostDescription,
        DurationUnit,
        DurationValue,
        StudyMode,
        AttendancePattern,
        [National]
    FROM #CourseRuns
) AS source
ON target.CourseRunId = source.CourseRunId
WHEN NOT MATCHED THEN
    INSERT (
        CourseRunId,
        CourseId,
        CourseRunStatus,
        CreatedOn,
        CreatedBy,
        UpdatedOn,
        UpdatedBy,
        CourseName,
        VenueId,
        ProviderCourseId,
        DeliveryMode,
        FlexibleStartDate,
        StartDate,
        CourseWebsite,
        Cost,
        CostDescription,
        DurationUnit,
        DurationValue,
        StudyMode,
        AttendancePattern,
        [National]
    ) VALUES (
        source.CourseRunId,
        source.CourseId,
        source.CourseRunStatus,
        source.CreatedOn,
        source.CreatedBy,
        source.UpdatedOn,
        source.UpdatedBy,
        source.CourseName,
        source.VenueId,
        source.ProviderCourseId,
        source.DeliveryMode,
        source.FlexibleStartDate,
        source.StartDate,
        source.CourseWebsite,
        CONVERT(DECIMAL, source.Cost) / 100,
        source.CostDescription,
        source.DurationUnit,
        source.DurationValue,
        source.StudyMode,
        source.AttendancePattern,
        source.[National]
    )
WHEN MATCHED THEN
    UPDATE SET
        CourseId = source.CourseId,
        CourseRunStatus = source.CourseRunStatus,
        CreatedOn = source.CreatedOn,
        CreatedBy = source.CreatedBy,
        UpdatedOn = source.UpdatedOn,
        UpdatedBy = source.UpdatedBy,
        CourseName = source.CourseName,
        VenueId = source.VenueId,
        ProviderCourseId = source.ProviderCourseId,
        DeliveryMode = source.DeliveryMode,
        FlexibleStartDate = source.FlexibleStartDate,
        StartDate = source.StartDate,
        CourseWebsite = source.CourseWebsite,
        Cost = CONVERT(DECIMAL, source.Cost) / 100,
        CostDescription = source.CostDescription,
        DurationUnit = source.DurationUnit,
        DurationValue = source.DurationValue,
        StudyMode = source.StudyMode,
        AttendancePattern = source.AttendancePattern,
        [National] = source.[National]
WHEN NOT MATCHED BY SOURCE AND target.CourseId IN (SELECT CourseId FROM #Courses) THEN DELETE;";

                await transaction.Connection.ExecuteAsync(mergeSql, transaction: transaction);
            }

            async Task UpsertCourseRunRegions()
            {
                var createTableSql = @"
CREATE TABLE #CourseRunRegions (
    CourseRunId UNIQUEIDENTIFIER,
    RegionId VARCHAR(9) COLLATE SQL_Latin1_General_CP1_CI_AS
)";

                await transaction.Connection.ExecuteAsync(createTableSql, transaction: transaction);

                await BulkCopyHelper.WriteRecords(
                    query.Records.SelectMany(c => c.CourseRuns.SelectMany(cr => cr.RegionIds.Select(r => new
                    {
                        cr.CourseRunId,
                        RegionId = r
                    }))),
                    tableName: "#CourseRunRegions",
                    transaction);

                var mergeSql = @"
MERGE Pttcd.CourseRunRegions AS target
USING (
    SELECT CourseRunId, RegionId FROM #CourseRunRegions
) AS source
ON target.CourseRunId = source.CourseRunId AND target.RegionId = source.RegionId
WHEN NOT MATCHED THEN
    INSERT (CourseRunId, RegionId) VALUES (source.CourseRunId, source.RegionId)
WHEN NOT MATCHED BY SOURCE AND target.CourseRunId IN (SELECT CourseRunId FROM #CourseRuns) THEN DELETE;";

                await transaction.Connection.ExecuteAsync(mergeSql, transaction: transaction);
            }

            async Task UpsertCourseRunSubRegions()
            {
                var createTableSql = @"
CREATE TABLE #CourseRunSubRegions (
    CourseRunId UNIQUEIDENTIFIER,
    RegionId VARCHAR(9) COLLATE SQL_Latin1_General_CP1_CI_AS
)";

                await transaction.Connection.ExecuteAsync(createTableSql, transaction: transaction);

                await BulkCopyHelper.WriteRecords(
                    query.Records.SelectMany(c => c.CourseRuns.SelectMany(cr => cr.SubRegionIds.Select(r => new
                    {
                        cr.CourseRunId,
                        RegionId = r
                    }))),
                    tableName: "#CourseRunSubRegions",
                    transaction);

                var mergeSql = @"
MERGE Pttcd.CourseRunSubRegions AS target
USING (
    SELECT CourseRunId, RegionId FROM #CourseRunSubRegions
) AS source
ON target.CourseRunId = source.CourseRunId AND target.RegionId = source.RegionId
WHEN NOT MATCHED THEN
    INSERT (CourseRunId, RegionId) VALUES (source.CourseRunId, source.RegionId)
WHEN NOT MATCHED BY SOURCE AND target.CourseRunId IN (SELECT CourseRunId FROM #CourseRuns) THEN DELETE;";

                await transaction.Connection.ExecuteAsync(mergeSql, transaction: transaction);
            }

            return new None();
        }
    }
}
