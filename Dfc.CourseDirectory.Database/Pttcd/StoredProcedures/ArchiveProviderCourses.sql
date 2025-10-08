CREATE PROCEDURE [Pttcd].[ArchiveProviderCourses]
	@RetentionDate datetime
AS
BEGIN

	DECLARE @NoneProviderType INT = 0;
	DECLARE @ArchivedCourseStatus INT = 4;
	DECLARE @UpdatedOn DATETIME = GETUTCDATE(); 
	DECLARE @UpdatedBy VARCHAR(100) = 'Stored Procedure - ArchiveProviderCourses';

	SELECT 
		c.CourseId, cr.CourseRunId
	INTO #TempCourseTable
	FROM 
		Pttcd.Providers p
		INNER JOIN Pttcd.Courses c ON c.ProviderId = p.ProviderId
		INNER JOIN Pttcd.CourseRuns cr ON cr.CourseId = c.CourseId
	WHERE 
		ProviderType = @NoneProviderType
		AND p.UpdatedOn < @RetentionDate
		AND CourseStatus <> @ArchivedCourseStatus
		AND CourseRunStatus <> @ArchivedCourseStatus



	UPDATE 
		Pttcd.CourseRuns
	SET
		CourseRunStatus = @ArchivedCourseStatus,
		UpdatedOn = @UpdatedOn,
		UpdatedBy = @UpdatedBy
	WHERE 
		CourseRunId IN (SELECT DISTINCT CourseRunId FROM #TempCourseTable)
			


	UPDATE 
		c
	SET
		c.CourseStatus = @ArchivedCourseStatus,
		c.UpdatedOn = @UpdatedOn,
		c.UpdatedBy = @UpdatedBy
	FROM Pttcd.Providers p
	INNER JOIN Pttcd.Courses c ON c.ProviderId = p.ProviderId
	WHERE 
		ProviderType = @NoneProviderType
		AND p.UpdatedOn < @RetentionDate
		AND CourseStatus <> @ArchivedCourseStatus



	DECLARE @CourseRunIds Pttcd.GuidIdTable

	INSERT INTO @CourseRunIds
	SELECT DISTINCT CourseRunId FROM #TempCourseTable

	EXEC Pttcd.RefreshFindACourseIndex @CourseRunIds = @CourseRunIds, @Now = @UpdatedOn

	DROP TABLE #TempCourseTable

END
