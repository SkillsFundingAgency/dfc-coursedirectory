using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class PublishCourseUploadHandler : ISqlQueryHandler<PublishCourseUpload, OneOf<NotFound, PublishCourseUploadResult>>
    {
        public async Task<OneOf<NotFound, PublishCourseUploadResult>> Execute(SqlTransaction transaction, PublishCourseUpload query)
        {
            var sql = $@"
UPDATE Pttcd.CourseUploads
SET UploadStatus = {(int)UploadStatus.Published}, PublishedOn = @PublishedOn
WHERE CourseUploadId = @CourseUploadId

IF @@ROWCOUNT = 0
BEGIN
    SELECT 0 AS Status
    RETURN
END

DECLARE @ProviderUkprn INT

SELECT @ProviderUkprn = Ukprn FROM Pttcd.Providers p
JOIN Pttcd.CourseUploads cu ON p.ProviderId = cu.ProviderId
WHERE cu.CourseUploadId = @CourseUploadId


-- Archive all existing courses

DECLARE @CourseIds Pttcd.GuidIdTable

INSERT INTO @CourseIds
SELECT CourseId FROM Pttcd.Courses
WHERE ProviderUkprn = @ProviderUkprn
AND (CourseStatus & (~ {(int)CourseStatus.Archived})) <> 0

UPDATE Pttcd.Courses
SET CourseStatus = {(int)CourseStatus.Archived}
FROM Pttcd.Courses c
JOIN @CourseIds x ON c.CourseId = x.Id

UPDATE Pttcd.CourseRuns 
SET CourseRunStatus = {(int)CourseStatus.Archived}
FROM Pttcd.CourseRuns cr
JOIN @CourseIds x ON cr.CourseId = x.Id
WHERE cr.CourseRunStatus <> {(int)CourseStatus.Archived}

UPDATE Pttcd.FindACourseIndex
SET Live = 0
FROM Pttcd.FindACourseIndex i
JOIN @CourseIds x ON i.CourseId = x.Id


-- Create new courses from the Course Upload's rows

;WITH CoursesCte AS (
    SELECT
        CourseId,
        LearnAimRef,
        WhoThisCourseIsFor,
        EntryRequirements,
        WhatYouWillLearn,
        HowYouWillLearn,
        WhatYouWillNeedToBring,
        HowYouWillBeAssessed,
        WhereNext,
        ROW_NUMBER() OVER (PARTITION BY CourseId ORDER BY RowNumber) AS GroupRowNumber
    FROM Pttcd.CourseUploadRows
    WHERE CourseUploadId = @CourseUploadId
    AND CourseUploadRowStatus = {(int)UploadRowStatus.Default}
)
INSERT INTO Pttcd.Courses (
    CourseId,
    CourseStatus,
    CreatedOn,
    CreatedBy,
    UpdatedOn,
    UpdatedBy,
    LearnAimRef,
    ProviderUkprn,
    CourseDescription,
    EntryRequirements,
    WhatYoullLearn,
    HowYoullLearn,
    WhatYoullNeed,
    HowYoullBeAssessed,
    WhereNext,
    DataIsHtmlEncoded
)
SELECT
    CourseId,
    {(int)CourseStatus.Live},
    @PublishedOn,
    @PublishedByUserId,
    @PublishedOn,
    @PublishedByUserId,
    LearnAimRef,
    @ProviderUkprn,
    WhoThisCourseIsFor,
    EntryRequirements,
    WhatYouWillLearn,
    HowYouWillLearn,
    WhatYouWillNeedToBring,
    HowYouWillBeAssessed,
    WhereNext,
    0  -- DataIsHtmlEncoded
FROM CoursesCte
WHERE GroupRowNumber = 1

INSERT INTO Pttcd.CourseRuns (
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
    DataIsHtmlEncoded
)
SELECT
    CourseRunId,
    CourseId,
    {(int)CourseStatus.Live},
    @PublishedOn,
    @PublishedByUserId,
    @PublishedOn,
    @PublishedByUserId,
    CourseName,
    VenueId,
    ProviderCourseRef,
    ResolvedDeliveryMode,
    ResolvedFlexibleStartDate,
    ResolvedStartDate,
    CourseWebpage,
    ResolvedCost,
    CostDescription,
    ResolvedDurationUnit,
    ResolvedDuration,
    ResolvedStudyMode,
    ResolvedAttendancePattern,
    ResolvedNationalDelivery,
    0  -- DataIsHtmlEncoded
FROM Pttcd.CourseUploadRows
WHERE CourseUploadId = @CourseUploadId
AND CourseUploadRowStatus = {(int)UploadRowStatus.Default}

INSERT INTO Pttcd.CourseRunSubRegions (CourseRunId, RegionId)
SELECT cur.CourseRunId, sr.RegionId
FROM Pttcd.CourseUploadRows cur
JOIN Pttcd.CourseUploadRowSubRegions sr ON cur.RowNumber = sr.RowNumber AND cur.CourseUploadId = sr.CourseUploadId
WHERE cur.CourseUploadId = @CourseUploadId
AND cur.CourseUploadRowStatus = {(int)UploadRowStatus.Default}


DECLARE @AddedCourseRunIds Pttcd.GuidIdTable

INSERT INTO @AddedCourseRunIds
SELECT CourseRunId FROM Pttcd.CourseUploadRows
WHERE CourseUploadId = @CourseUploadId
AND CourseUploadRowStatus = {(int)UploadRowStatus.Default}

EXEC Pttcd.RefreshFindACourseIndex @CourseRunIds = @AddedCourseRunIds, @Now = @PublishedOn


SELECT 1 AS Status

SELECT COUNT(*) FROM @AddedCourseRunIds
";

            var paramz = new
            {
                query.CourseUploadId,
                query.PublishedOn,
                PublishedByUserId = query.PublishedBy.UserId
            };

            using var reader = await transaction.Connection.QueryMultipleAsync(sql, paramz, transaction);

            var status = await reader.ReadSingleAsync<int>();

            if (status == 1)
            {
                var publishedCourseRunCount = await reader.ReadSingleAsync<int>();

                return new PublishCourseUploadResult()
                {
                    PublishedCount = publishedCourseRunCount
                };
            }
            else
            {
                return new NotFound();
            }
        }
    }
}
