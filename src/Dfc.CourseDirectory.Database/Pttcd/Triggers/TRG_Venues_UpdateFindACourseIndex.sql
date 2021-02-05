CREATE TRIGGER [Pttcd].[TRG_Venues_UpdateFindACourseIndex]
ON [Pttcd].[Venues]
AFTER UPDATE
AS
BEGIN

	DECLARE @VenueLiveCourseRuns Pttcd.GuidIdTable,
			@ProviderLiveTLevels Pttcd.GuidIdTable,
			@Now DATETIME

	INSERT INTO @VenueLiveCourseRuns
	SELECT cr.CourseRunId FROM Pttcd.CourseRuns cr
	JOIN inserted x ON cr.VenueId = x.VenueId
	WHERE cr.CourseRunStatus = 1

	INSERT INTO @ProviderLiveTLevels
	SELECT t.TLevelId FROM Pttcd.TLevels t
	JOIN Pttcd.TLevelLocations tll ON t.TLevelId = tll.TLevelId
	JOIN inserted x ON tll.VenueId = x.VenueId
	WHERE t.TLevelStatus = 1 AND tll.TLevelLocationStatus = 1

	SET @Now = GETUTCDATE()

	EXEC Pttcd.RefreshFindACourseIndex @VenueLiveCourseRuns, @Now

	EXEC Pttcd.RefreshFindACourseIndexForTLevels @ProviderLiveTLevels, @Now

END
