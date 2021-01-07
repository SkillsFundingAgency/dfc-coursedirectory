﻿CREATE TRIGGER [Pttcd].[TRG_Providers_UpdateFindACourseIndex]
ON [Pttcd].[Providers]
AFTER UPDATE
AS
BEGIN

	DECLARE @ProviderLiveCourseRuns Pttcd.GuidIdTable,
			@Now DATETIME

	INSERT INTO @ProviderLiveCourseRuns
	SELECT cr.CourseRunId FROM Pttcd.CourseRuns cr
	JOIN Pttcd.Courses c ON cr.CourseId = c.CourseId
	JOIN inserted x ON c.ProviderUkprn = x.Ukprn
	WHERE cr.CourseRunStatus = 1

	SET @Now = GETUTCDATE()

	EXEC Pttcd.RefreshFindACourseIndex @ProviderLiveCourseRuns, @Now

END
