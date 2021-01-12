CREATE TRIGGER [Pttcd].[TRG_Providers_UpdateFindACourseIndex]
ON [Pttcd].[Providers]
AFTER UPDATE
AS
BEGIN

	DECLARE @ProviderLiveCourseRuns Pttcd.GuidIdTable

	INSERT INTO @ProviderLiveCourseRuns
	SELECT cr.CourseRunId FROM Pttcd.CourseRuns cr
	JOIN Pttcd.Courses c ON cr.CourseId = c.CourseId
	JOIN inserted x ON c.ProviderUkprn = x.Ukprn
	JOIN deleted y ON y.ProviderId = x.ProviderId
	WHERE cr.CourseRunStatus = 1
	-- Check that at least one mutable field used by FAC index has actually changed
	AND (y.Alias <> x.Alias
		OR y.ProviderName <> x.ProviderName
		OR y.DisplayNameSource <> x.DisplayNameSource)

	EXEC Pttcd.RefreshFindACourseIndex @ProviderLiveCourseRuns

END
