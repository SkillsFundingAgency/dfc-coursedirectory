-- This view is the data source of the Azure Search index that powers the FAC API
CREATE VIEW [Pttcd].[FindACourseIndex] WITH SCHEMABINDING
	AS SELECT
		-- Id is the CourseRun's ID appended with the Region, if there is one
		CONVERT(VARCHAR(36), cr.CourseRunId) + CASE WHEN crr.RegionId IS NOT NULL THEN '-' + crr.RegionId ELSE '' END AS Id,

		1 AS OfferingType, -- Course

		c.CourseId,
		cr.CourseRunId,
		CAST(CASE WHEN cr.CourseRunStatus = 1 THEN 1 ELSE 0 END AS BIT) AS Live,

		-- Use the max version from Course, Provider and Venue
		(SELECT MAX(AllVersions.[Version]) FROM (
			VALUES (c.[Version]), (p.[Version]), (v.[Version])) AS AllVersions([Version])
		) AS [Version],

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

		r.Name AS RegionName,

		-- Magic numbers and logic from https://github.com/SkillsFundingAgency/dfc-providerportal-changefeedlistener/commit/608340dcfaa5c74ee8b1ae422ad902ee0c529c01#diff-5f9ef9c9ca0b0bc9af8b5c7926cfcfe31fa7e8367b9104357104ad568fbe0302R103-R104
		CAST(CASE
			WHEN r.RegionId IS NULL THEN 1
			WHEN r.ParentRegionId IS NOT NULL THEN 4.5  -- Sub region
			ELSE 2.3  -- Region
			END AS float) AS ScoreBoost
	FROM Pttcd.Courses c
	INNER JOIN Pttcd.CourseRuns cr ON c.CourseId = cr.CourseId
	LEFT JOIN Pttcd.CourseRunSubRegions crr ON cr.CourseRunId = crr.CourseRunId
	LEFT JOIN Pttcd.Regions r ON crr.RegionId = r.RegionId
	INNER JOIN Pttcd.Providers p ON c.ProviderUkprn = p.Ukprn
	INNER JOIN LARS.LearningDelivery ld ON c.LearnAimRef = ld.LearnAimRef
	LEFT JOIN Pttcd.Venues v ON cr.VenueId = v.VenueId
