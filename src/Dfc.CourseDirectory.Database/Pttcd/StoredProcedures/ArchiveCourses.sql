CREATE PROCEDURE [Pttcd].[ArchiveCourses]	
AS
BEGIN

	DECLARE @ArchivedCourseRunStatus INT = 4;
	DECLARE @LiveCourseStatus INT = 1;
	DECLARE @UpdatedOn DATETIME = GETUTCDATE(); 
	DECLARE @UpdatedBy VARCHAR(100) = 'Stored Procedure - ArchiveCourses';


	BEGIN TRY
		BEGIN TRANSACTION

		SELECT 
			c.CourseId, cr.CourseRunId, cr.CourseRunStatus
		INTO #TempTable
		FROM 
			[Pttcd].[Courses] c
			INNER JOIN Pttcd.CourseRuns cr ON cr.CourseId = c.CourseId
			INNER JOIN Pttcd.FindACourseIndex fac ON fac.CourseRunId = cr.CourseRunId
		WHERE 
			cr.StartDate < DATEADD(MONTH, -15, GETUTCDATE()) AND cr.CourseRunStatus <> @ArchivedCourseRunStatus and fac.Live = @LiveCourseStatus

		--Select * from #TempTable

		UPDATE 
			Pttcd.CourseRuns
		SET
			CourseRunStatus = @ArchivedCourseRunStatus,
			UpdatedOn = @UpdatedOn,
			UpdatedBy = @UpdatedBy
		WHERE 
			CourseRunId IN (SELECT DISTINCT CourseRunId FROM #TempTable)



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

		--Select * from #TempCourseStatus

		-- Also update courses status. 
		-- Remember, Course status 5 indicates that the course has at least one CourseRun record with a CourseRunStatus of 1 and at least one CourseRun record with a CourseRunStatus of 4.
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

	COMMIT TRAN
	END TRY
	BEGIN CATCH
		DECLARE 
			@ErrorMessage NVARCHAR(4000),
			@ErrorSeverity INT,
			@ErrorState INT;
		SELECT 
			@ErrorMessage = ERROR_MESSAGE(),
			@ErrorSeverity = ERROR_SEVERITY(),
			@ErrorState = ERROR_STATE();
		RAISERROR (
			@ErrorMessage,
			@ErrorSeverity,
			@ErrorState    
			);

		IF(@@TRANCOUNT > 0)
		ROLLBACK TRAN;    
	END CATCH

END
