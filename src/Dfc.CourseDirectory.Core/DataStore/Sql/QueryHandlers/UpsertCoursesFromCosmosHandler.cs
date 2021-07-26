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
            await UpsertCourseBulkUploadErrors();
            await UpsertCourseRuns();
            await UpsertCourseRunBulkUploadErrors();
            await UpsertCourseRunRegions();
            await UpsertCourseRunSubRegions();
            await UpdateFindACourseIndex();

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
    WhereNext NVARCHAR(MAX),
    BulkUploadErrorCount INT
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
                        r.WhereNext,
                        BulkUploadErrorCount = r.BulkUploadErrors.Count()
                    }),
                    tableName: "#Courses",
                    transaction);

                var mergeSql = @"
MERGE Pttcd.Courses AS target
USING (
    SELECT
        c.CourseId,
        p.ProviderId,
        c.CourseStatus,
        c.CreatedOn,
        c.CreatedBy,
        c.UpdatedOn,
        c.UpdatedBy,
        c.TribalCourseId,
        c.LearnAimRef,
        c.ProviderUkprn,
        c.CourseDescription,
        c.EntryRequirements,
        c.WhatYoullLearn,
        c.HowYoullLearn,
        c.WhatYoullNeed,
        c.HowYoullBeAssessed,
        c.WhereNext,
        c.BulkUploadErrorCount
    FROM #Courses c
    JOIN Pttcd.Providers p ON c.ProviderUkprn = p.Ukprn
) AS source
ON target.CourseId = source.CourseId
WHEN NOT MATCHED THEN
    INSERT (
        CourseId,
        ProviderId,
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
        WhereNext,
        BulkUploadErrorCount,
        DataIsHtmlEncoded
    ) VALUES (
        source.CourseId,
        source.ProviderId,
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
        source.WhereNext,
        source.BulkUploadErrorCount,
        1
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
        WhereNext = source.WhereNext,
        BulkUploadErrorCount = source.BulkUploadErrorCount;";

                await transaction.Connection.ExecuteAsync(
                    mergeSql,
                    param: new { query.LastSyncedFromCosmos },
                    transaction: transaction);
            }

            async Task UpsertCourseBulkUploadErrors()
            {
                var createTableSql = @"
CREATE TABLE #CourseBulkUploadErrors (
    CourseId UNIQUEIDENTIFIER,
	CourseBulkUploadErrorIndex INT NOT NULL,
	LineNumber INT,
	Header NVARCHAR(MAX),
	Error NVARCHAR(MAX),
)";

                await transaction.Connection.ExecuteAsync(createTableSql, transaction: transaction);

                await BulkCopyHelper.WriteRecords(
                    query.Records.SelectMany(cr => cr.BulkUploadErrors.Select((e, i) => new
                    {
                        cr.CourseId,
                        CourseBulkUploadErrorIndex = i,
                        e.LineNumber,
                        e.Header,
                        e.Error
                    })),
                    tableName: "#CourseBulkUploadErrors",
                    transaction);

                var mergeSql = @"
MERGE Pttcd.CourseBulkUploadErrors AS target
USING (
    SELECT * FROM #CourseBulkUploadErrors
) AS source
ON target.CourseId = source.CourseId AND target.CourseBulkUploadErrorIndex = source.CourseBulkUploadErrorIndex
WHEN MATCHED THEN
    UPDATE SET LineNumber = source.LineNumber, Header = source.Header, Error = source.Error
WHEN NOT MATCHED THEN
    INSERT (CourseId, CourseBulkUploadErrorIndex, LineNumber, Header, Error)
    VALUES (source.CourseId, source.CourseBulkUploadErrorIndex, source.LineNumber, source.Header, source.Error)
WHEN NOT MATCHED BY SOURCE AND target.CourseId IN (SELECT CourseId FROM #CourseBulkUploadErrors) THEN DELETE;";

                await transaction.Connection.ExecuteAsync(mergeSql, transaction: transaction);
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
    [National] BIT,
    BulkUploadErrorCount INT
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
                        cr.AttendancePattern,
                        cr.National,
                        BulkUploadErrorCount = cr.BulkUploadErrors.Count()
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
        [National],
        BulkUploadErrorCount
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
        [National],
        BulkUploadErrorCount,
        DataIsHtmlEncoded
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
        source.[National],
        source.BulkUploadErrorCount,
        1
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
        [National] = source.[National],
        BulkUploadErrorCount = source.BulkUploadErrorCount
WHEN NOT MATCHED BY SOURCE AND target.CourseId IN (SELECT CourseId FROM #Courses) THEN DELETE;";

                await transaction.Connection.ExecuteAsync(mergeSql, transaction: transaction);
            }

            async Task UpsertCourseRunBulkUploadErrors()
            {
                var createTableSql = @"
CREATE TABLE #CourseRunBulkUploadErrors (
    CourseRunId UNIQUEIDENTIFIER,
	CourseRunBulkUploadErrorIndex INT NOT NULL,
	LineNumber INT,
	Header NVARCHAR(MAX),
	Error NVARCHAR(MAX),
)";

                await transaction.Connection.ExecuteAsync(createTableSql, transaction: transaction);

                await BulkCopyHelper.WriteRecords(
                    query.Records.SelectMany(c => c.CourseRuns).SelectMany(cr => cr.BulkUploadErrors.Select((e, i) => new
                    {
                        cr.CourseRunId,
                        CourseRunBulkUploadErrorIndex = i,
                        e.LineNumber,
                        e.Header,
                        e.Error
                    })),
                    tableName: "#CourseRunBulkUploadErrors",
                    transaction);

                var mergeSql = @"
MERGE Pttcd.CourseRunBulkUploadErrors AS target
USING (
    SELECT * FROM #CourseRunBulkUploadErrors
) AS source
ON target.CourseRunId = source.CourseRunId AND target.CourseRunBulkUploadErrorIndex = source.CourseRunBulkUploadErrorIndex
WHEN MATCHED THEN
    UPDATE SET LineNumber = source.LineNumber, Header = source.Header, Error = source.Error
WHEN NOT MATCHED THEN
    INSERT (CourseRunId, CourseRunBulkUploadErrorIndex, LineNumber, Header, Error)
    VALUES (source.CourseRunId, source.CourseRunBulkUploadErrorIndex, source.LineNumber, source.Header, source.Error)
WHEN NOT MATCHED BY SOURCE AND target.CourseRunId IN (SELECT CourseRunId FROM #CourseRunBulkUploadErrors) THEN DELETE;";

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

            Task UpdateFindACourseIndex()
            {
                var courseRunIds = query.Records.SelectMany(c => c.CourseRuns).Select(cr => cr.CourseRunId);

                return transaction.Connection.ExecuteAsync(
                    "Pttcd.RefreshFindACourseIndex",
                    param: new
                    {
                        CourseRunIds = TvpHelper.CreateGuidIdTable(courseRunIds),
                        Now = query.LastSyncedFromCosmos
                    },
                    transaction: transaction,
                    commandType: System.Data.CommandType.StoredProcedure);
            }

            return new None();
        }
    }
}
