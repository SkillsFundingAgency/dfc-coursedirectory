CREATE PROCEDURE [Pttcd].[ArchiveOldCourses]
	@RetentionDate datetime
AS
BEGIN

	DECLARE @ArchivedCourseRunStatus INT = 4;
	DECLARE @LiveCourseStatus INT = 1;
	DECLARE @UpdatedOn DATETIME = GETUTCDATE(); 
	DECLARE @UpdatedBy VARCHAR(100) = 'Stored Procedure - ArchiveOldCourses';	

	

	SELECT 
		c.CourseId, cr.CourseRunId, cr.CourseRunStatus
	INTO #TempTable
	FROM 
		[Pttcd].[Courses] c
		INNER JOIN Pttcd.CourseRuns cr ON cr.CourseId = c.CourseId
		INNER JOIN Pttcd.FindACourseIndex fac ON fac.CourseRunId = cr.CourseRunId
	WHERE 
		cr.StartDate < @RetentionDate AND cr.CourseRunStatus <> @ArchivedCourseRunStatus and fac.Live = @LiveCourseStatus
		

	UPDATE 
		Pttcd.CourseRuns
	SET
		CourseRunStatus = @ArchivedCourseRunStatus,
		UpdatedOn = @UpdatedOn,
		UpdatedBy = @UpdatedBy
	WHERE 
		CourseRunId IN (SELECT DISTINCT CourseRunId FROM #TempTable)

	-- CourseStatus 
	-- 1 - Live
	-- 4 - Archived
	-- 5 - Indicates that the course has at least one CourseRun record with a CourseRunStatus of 1 and also at least one CourseRun record with a CourseRunStatus of 4.
	SELECT 
		c.CourseId, SUM(DISTINCT(cr.CourseRunStatus)) as CourseStatus
	INTO #TempCourseStatus
	FROM 
		[Pttcd].[Courses] c
		INNER JOIN Pttcd.CourseRuns cr ON cr.CourseId = c.CourseId
		INNER JOIN Pttcd.FindACourseIndex fac ON fac.CourseRunId = cr.CourseRunId
	WHERE 
		cr.CourseRunId IN (SELECT DISTINCT CourseRunId FROM #TempTable)	
	GROUP BY c.CourseId
		

	-- Also update courses status. 		
	UPDATE 
		c
	SET 
		CourseStatus = tcs.CourseStatus,
		UpdatedOn = @UpdatedOn,
		UpdatedBy = @UpdatedBy
	FROM 
		[Pttcd].[Courses] c
		INNER JOIN #TempCourseStatus tcs ON tcs.CourseId = c.CourseId


	DECLARE @CourseRunIds Pttcd.GuidIdTable

	INSERT INTO @CourseRunIds
	SELECT DISTINCT CourseRunId FROM #TempTable

	EXEC Pttcd.RefreshFindACourseIndex @CourseRunIds = @CourseRunIds, @Now = @UpdatedOn

	DROP TABLE #TempTable
	DROP TABLE #TempCourseStatus	

END
