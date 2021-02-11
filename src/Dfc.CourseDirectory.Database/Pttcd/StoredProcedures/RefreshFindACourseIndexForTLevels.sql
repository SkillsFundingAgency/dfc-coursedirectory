CREATE PROCEDURE [Pttcd].[RefreshFindACourseIndexForTLevels]
	@TLevelIds [Pttcd].[GuidIdTable] READONLY,
	@Now DATETIME
AS
BEGIN

	MERGE Pttcd.FindACourseIndex AS target
	USING (
		SELECT
			CONVERT(VARCHAR(36), tll.TLevelLocationId) AS Id,
			2 AS OfferingType, -- TLevel
			t.TLevelId,
			tll.TLevelLocationId,
			t.UpdatedOn,
			p.ProviderId,
			CASE WHEN p.DisplayNameSource = 1 /* TradingName */ THEN ISNULL(NULLIF(p.Alias, ''), p.ProviderName) ELSE p.ProviderName END AS ProviderDisplayName,
			p.Ukprn ProviderUkprn,
			tld.Name QualificationCourseTitle,
			NULL LearnAimRef,
			tld.QualificationLevel NotionalNVQLevelv2,
			t.WhoFor CourseDescription,
			tld.Name CourseName,
			1 DeliveryMode,  -- Classroom based,
			CAST(0 AS bit) FlexibleStartDate,
			t.StartDate,
			NULL Cost,
			'T Levels are currently only available to 16-19 year olds. Contact us for details of other suitable courses.' CostDescription,
			4 DurationUnit,  -- Years,
			2 DurationValue,
			1 StudyMode,  -- Full time,
			1 AttendancePattern, -- Day time,
			NULL [National],
			v.VenueName,
			STUFF(
				CONCAT(
					NULLIF(', ' + v.AddressLine1, ', '),
					NULLIF(', ' + v.AddressLine2, ', '),
					NULLIF(', ' + v.Town, ', '),
					NULLIF(', ' + v.County, ', '),
					NULLIF(', ' + v.Postcode, ', ')),
				1, 2, NULL) AS VenueAddress,
			v.Town AS VenueTown,
			v.Position,
			NULL RegionName,

		--	-- Magic numbers and logic from https://github.com/SkillsFundingAgency/dfc-providerportal-changefeedlistener/commit/608340dcfaa5c74ee8b1ae422ad902ee0c529c01#diff-5f9ef9c9ca0b0bc9af8b5c7926cfcfe31fa7e8367b9104357104ad568fbe0302R103-R104
		1 AS ScoreBoost
		FROM @TLevelIds d
		INNER JOIN Pttcd.TLevels t ON d.Id = t.TLevelId
		INNER JOIN Pttcd.TLevelLocations tll ON t.TLevelId = tll.TLevelId
		INNER JOIN Pttcd.TLevelDefinitions tld ON t.TLevelDefinitionId = tld.TLevelDefinitionId
		INNER JOIN Pttcd.Providers p ON t.ProviderId = p.ProviderId
		INNER JOIN Pttcd.Venues v ON tll.VenueId = v.VenueId
		WHERE t.TLevelStatus = 1 AND tll.TLevelLocationStatus = 1  -- Live
	) AS source
	ON source.Id = target.Id
	WHEN MATCHED THEN UPDATE SET
		LastSynced = @Now,
		OfferingType = source.OfferingType,
		TLevelId = source.TLevelId,
		TLevelLocationId = source.TLevelLocationId,
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
		LastSynced,
		OfferingType,
		TLevelId,
		TLevelLocationId,
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
		@Now,
		source.OfferingType,
		source.TLevelId,
		source.TLevelLocationId,
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
	WHEN NOT MATCHED BY SOURCE AND target.TLevelId IN (SELECT Id FROM @TLevelIds) THEN UPDATE SET
		Live = 0,
		LastSynced = @Now;

END
