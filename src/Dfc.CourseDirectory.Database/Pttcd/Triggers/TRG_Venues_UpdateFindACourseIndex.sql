CREATE TRIGGER [Pttcd].[TRG_Venues_UpdateFindACourseIndex]
ON [Pttcd].[Venues]
AFTER UPDATE
AS
BEGIN

	DECLARE @VenueLiveCourseRuns Pttcd.GuidIdTable,
			@Now DATETIME

	INSERT INTO @VenueLiveCourseRuns
	SELECT cr.CourseRunId FROM Pttcd.CourseRuns cr
	JOIN inserted x ON cr.VenueId = x.VenueId
	WHERE cr.CourseRunStatus = 1

	SET @Now = GETUTCDATE()

	EXEC Pttcd.RefreshFindACourseIndex @VenueLiveCourseRuns, @Now

END
