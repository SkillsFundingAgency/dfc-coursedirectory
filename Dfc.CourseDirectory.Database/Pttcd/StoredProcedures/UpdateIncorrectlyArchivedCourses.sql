CREATE PROCEDURE [Pttcd].[UpdateIncorrectlyArchivedCourses]
AS
BEGIN

	DECLARE @ArchivedCourseStatus INT = 4;
	DECLARE @LiveCourseStatus INT = 1;
	DECLARE @UpdatedOn DATETIME = GETUTCDATE(); 
	DECLARE @UpdatedBy VARCHAR(100) = 'Stored Procedure - UpdateIncorrectlyArchivedCourses';	

	

	SELECT 
		CourseId,
		CourseRunId
	INTO #TempTable
	FROM 
		[Pttcd].[FindACourseIndex]
	WHERE 
		CourseId IN ( SELECT CourseId FROM [Pttcd].Courses WHERE CourseStatus = @ArchivedCourseStatus) AND Live = @LiveCourseStatus
		

	UPDATE 
		Pttcd.Courses
	SET
		CourseStatus = @LiveCourseStatus,
		UpdatedOn = @UpdatedOn,
		UpdatedBy = @UpdatedBy
	WHERE 
		CourseId IN (SELECT CourseId FROM #TempTable)


	DROP TABLE #TempTable

END
