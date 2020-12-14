CREATE TRIGGER [Pttcd].[TRG_Venues_UpdateFindACourseIndex]
ON [Pttcd].[Venues]
AFTER UPDATE
AS
BEGIN

	DECLARE @VenueLiveCourseRuns Pttcd.GuidIdTable

	INSERT INTO @VenueLiveCourseRuns
	SELECT cr.CourseRunId FROM Pttcd.CourseRuns cr
	JOIN Pttcd.Courses c ON cr.CourseId = c.CourseId
	JOIN inserted x ON c.ProviderUkprn = x.ProviderUkprn
	WHERE cr.CourseRunStatus = 1

	EXEC Pttcd.RefreshFindACourseIndex @VenueLiveCourseRuns

END
