CREATE PROCEDURE [Pttcd].[RemoveRedundantRecords]
	@RetentionDate datetime
AS
BEGIN

	DECLARE @NonLiveStatus int = 0
	DECLARE @ArchivedStatus int = 4
	DECLARE @MaxNoOfRecordsToDelete int = 10000


	DELETE TOP (@MaxNoOfRecordsToDelete)
		crr 
	FROM 
		Pttcd.CourseRuns cr
		INNER JOIN Pttcd.CourseRunRegions crr ON crr.CourseRunId = cr.CourseRunId
	WHERE 
		cr.CourseRunStatus = @ArchivedStatus 
		AND UpdatedOn < @RetentionDate


	DELETE TOP (@MaxNoOfRecordsToDelete)
		crsr
	FROM 
		Pttcd.CourseRuns cr
		INNER JOIN Pttcd.CourseRunSubRegions crsr ON crsr.CourseRunId = cr.CourseRunId
	WHERE 
		CourseRunStatus = @ArchivedStatus 
		AND UpdatedOn < @RetentionDate


	DELETE TOP (@MaxNoOfRecordsToDelete)
		crbue 
	FROM 
		Pttcd.CourseRuns cr
		INNER JOIN Pttcd.CourseRunBulkUploadErrors crbue on crbue.CourseRunId = cr.CourseRunId
	WHERE 
		cr.CourseRunStatus = @ArchivedStatus


	DELETE TOP (@MaxNoOfRecordsToDelete)
	FROM 
		Pttcd.CourseRuns 
	WHERE 
		CourseRunStatus = @ArchivedStatus 
		AND UpdatedOn < @RetentionDate
		AND CourseRunId NOT IN (Select Distinct CourseRunId From Pttcd.CourseRunRegions)
		AND CourseRunId NOT IN (Select Distinct CourseRunId From Pttcd.CourseRunSubRegions)


	DELETE TOP (@MaxNoOfRecordsToDelete)
		cbue 
	FROM 
		Pttcd.Courses c
		INNER JOIN Pttcd.CourseBulkUploadErrors cbue on cbue.CourseId = c.CourseId
	WHERE 
		c.CourseStatus = @ArchivedStatus


	DELETE TOP (@MaxNoOfRecordsToDelete)
	FROM 
		Pttcd.Courses 
	WHERE 
		CourseStatus = @ArchivedStatus 
		and UpdatedOn < @RetentionDate
		and CourseId NOT IN (Select Distinct CourseId From Pttcd.CourseRuns)


	DELETE TOP (@MaxNoOfRecordsToDelete)
	FROM 
		Pttcd.FindACourseIndex 
	WHERE 
		Live = @NonLiveStatus 
		AND LastSynced < @RetentionDate

END
