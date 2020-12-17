using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class PopulateFindACourseIndexTable
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public PopulateFindACourseIndexTable(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        [FunctionName(nameof(PopulateFindACourseIndexTable))]
        [NoAutomaticTrigger]
        [Singleton]
        public Task Run(string input)
        {
            var sql = @"
WITH RecordsCte AS (
	SELECT
		-- Id is the CourseRun's ID appended with the Region, if there is one
		CONVERT(VARCHAR(36), cr.CourseRunId) + CASE WHEN crr.RegionId IS NOT NULL THEN '-' + crr.RegionId ELSE '' END AS Id,
		1 AS OfferingType, -- Course
		c.CourseId,
		cr.CourseRunId,
		c.UpdatedOn,
		p.ProviderId,
		p.ProviderName AS ProviderDisplayName,
		c.ProviderUkprn,
		ld.LearnAimRefTitle QualificationCourseTitle,
		c.LearnAimRef,
		ld.NotionalNVQLevelv2,
		c.CourseDescription,
		cr.CourseName,
		cr.DeliveryMode,
		cr.FlexibleStartDate,
		cr.StartDate,
		cr.Cost,
		cr.CostDescription,
		cr.DurationUnit,
		cr.DurationValue,
		cr.StudyMode,
		CASE WHEN cr.DeliveryMode = 1 THEN cr.AttendancePattern ELSE null END AttendancePattern,
		cr.[National],
		v.VenueName,

		CONCAT_WS(
			', ',
			NULLIF(v.AddressLine1, ''),
			NULLIF(v.AddressLine2, ''),
			NULLIF(v.Town, ''),
			NULLIF(v.County, ''),
			NULLIF(v.Postcode, '')
		) AS VenueAddress,

		v.Town AS VenueTown,
		COALESCE(v.Position, r.Position) AS Position,
		CASE WHEN r.Name IS NOT NULL THEN r.Name WHEN cr.[National] = 1 THEN 'National' ELSE '' END AS RegionName,

		-- Magic numbers and logic from https://github.com/SkillsFundingAgency/dfc-providerportal-changefeedlistener/commit/608340dcfaa5c74ee8b1ae422ad902ee0c529c01#diff-5f9ef9c9ca0b0bc9af8b5c7926cfcfe31fa7e8367b9104357104ad568fbe0302R103-R104
		CAST(CASE
			WHEN r.RegionId IS NULL THEN 1
			WHEN r.ParentRegionId IS NOT NULL THEN 4.5  -- Sub region
			ELSE 2.3  -- Region
			END AS float) AS ScoreBoost
	FROM Pttcd.CourseRuns cr
	INNER JOIN Pttcd.Courses c ON cr.CourseId = c.CourseId
	LEFT JOIN Pttcd.CourseRunSubRegions crr ON cr.CourseRunId = crr.CourseRunId  AND cr.[National] = 0
	LEFT JOIN Pttcd.Regions r ON crr.RegionId = r.RegionId
	INNER JOIN Pttcd.Providers p ON c.ProviderUkprn = p.Ukprn
	INNER JOIN LARS.LearningDelivery ld ON c.LearnAimRef = ld.LearnAimRef
	LEFT JOIN Pttcd.Venues v ON cr.VenueId = v.VenueId
	WHERE cr.CourseRunStatus = 1  -- Live
)
MERGE Pttcd.FindACourseIndex AS target
USING (SELECT * FROM RecordsCte) AS source
ON source.Id = target.Id
WHEN MATCHED THEN UPDATE SET
	OfferingType = source.OfferingType,
	CourseId = source.CourseId,
	CourseRunId = source.CourseRunId,
	Live = 1,
	UpdatedOn = source.UpdatedOn,
	ProviderId = source.ProviderId,
	ProviderDisplayName = source.ProviderDisplayName,
	ProviderUkprn = source.ProviderUkprn,
	QualificationCourseTitle = source.QualificationCourseTitle,
	LearnAimRef = source.LearnAimRef,
	NotionalNVQLevelv2 = source.NotionalNVQLevelv2,
	CourseDescription = source.CourseDescription,
	CourseName = source.CourseName,
	DeliveryMode = source.DeliveryMode,
	FlexibleStartDate = source.FlexibleStartDate,
	StartDate = source.StartDate,
	Cost = source.Cost,
	CostDescription = source.CostDescription,
	DurationUnit = source.DurationUnit,
	DurationValue = source.DurationValue,
	StudyMode = source.StudyMode,
	AttendancePattern = source.AttendancePattern,
	[National] = source.[National],
	VenueName = source.VenueName,
	VenueAddress = source.VenueAddress,
	VenueTown = source.VenueTown,
	Position = source.Position,
	RegionName = source.RegionName,
	ScoreBoost = source.ScoreBoost
WHEN NOT MATCHED THEN INSERT (
	Id,
	OfferingType,
	CourseId,
	CourseRunId,
	Live,
	UpdatedOn,
	ProviderId,
	ProviderDisplayName,
	ProviderUkprn,
	QualificationCourseTitle,
	LearnAimRef,
	NotionalNVQLevelv2,
	CourseDescription,
	CourseName,
	DeliveryMode,
	FlexibleStartDate,
	StartDate,
	Cost,
	CostDescription,
	DurationUnit,
	DurationValue,
	StudyMode,
	AttendancePattern,
	[National],
	VenueName,
	VenueAddress,
	VenueTown,
	Position,
	RegionName,
	ScoreBoost)
VALUES (
	source.Id,
	source.OfferingType,
	source.CourseId,
	source.CourseRunId,
	1,
	source.UpdatedOn,
	source.ProviderId,
	source.ProviderDisplayName,
	source.ProviderUkprn,
	source.QualificationCourseTitle,
	source.LearnAimRef,
	source.NotionalNVQLevelv2,
	source.CourseDescription,
	source.CourseName,
	source.DeliveryMode,
	source.FlexibleStartDate,
	source.StartDate,
	source.Cost,
	source.CostDescription,
	source.DurationUnit,
	source.DurationValue,
	source.StudyMode,
	source.AttendancePattern,
	source.[National],
	source.VenueName,
	source.VenueAddress,
	source.VenueTown,
	source.Position,
	source.RegionName,
	source.ScoreBoost)
WHEN NOT MATCHED BY SOURCE THEN UPDATE SET Live = 0;";

            return _sqlQueryDispatcher.Transaction.Connection.ExecuteAsync(
                sql,
                transaction: _sqlQueryDispatcher.Transaction,
                commandTimeout: 0);
        }
    }
}
