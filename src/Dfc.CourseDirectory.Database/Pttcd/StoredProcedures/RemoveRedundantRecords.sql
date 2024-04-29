CREATE PROCEDURE [Pttcd].[RemoveRedundantRecords]
	@RetentionDate datetime
AS
	DECLARE @NonLiveStatus int = 0
	DECLARE @ArchivedStatus int = 4
	DECLARE @UpdatedBy varchar(100) = 'RemoveRedundantRecordsStoredProc'
	DECLARE @MaxNoOfRecordsToRemove int = 2
	
	DELETE 		
	FROM 
		Pttcd.FindACourseIndex 
	WHERE 
		Live = @NonLiveStatus 
		AND LastSynced < @RetentionDate


	DELETE 
		crr 
	FROM 
		Pttcd.CourseRuns cr
		INNER JOIN Pttcd.CourseRunRegions crr ON crr.CourseRunId = cr.CourseRunId
	WHERE 
		cr.CourseRunStatus = @ArchivedStatus 
		AND UpdatedOn < @RetentionDate
		

	DELETE 
		crsr
	FROM 
		Pttcd.CourseRuns cr
		INNER JOIN Pttcd.CourseRunSubRegions crsr ON crsr.CourseRunId = cr.CourseRunId
	WHERE 
		CourseRunStatus = @ArchivedStatus 
		AND UpdatedOn < @RetentionDate


	DELETE 		
	FROM 
		Pttcd.CourseRuns 
	WHERE 
		CourseRunStatus = @ArchivedStatus 
		AND UpdatedOn < @RetentionDate
		AND CourseRunId NOT IN (Select Distinct CourseRunId From Pttcd.CourseRunRegions)
		AND CourseRunId NOT IN (Select Distinct CourseRunId From Pttcd.CourseRunSubRegions)

	DELETE 		
	FROM 
		Pttcd.Courses 
	WHERE 
		CourseStatus = @ArchivedStatus 
		and UpdatedOn < @RetentionDate
		and CourseId NOT IN (Select Distinct CourseId From Pttcd.CourseRuns)

	DELETE 
	FROM 
		Pttcd.Venues 
	WHERE 
		VenueStatus = @ArchivedStatus 
		AND UpdatedOn < @RetentionDate


	-- Some of the records in tables are archived but UpdatedOn fields are not set. Below statements set UpdatedOn field so that these records can also be deleted.

	UPDATE 		
		Pttcd.Courses 
	SET 
		UpdatedOn = GETDATE(),
		UpdatedBy = @UpdatedBy
	WHERE 
		CourseStatus = @ArchivedStatus 
		AND UpdatedOn IS NULL

	
	UPDATE 		
		Pttcd.CourseRuns 
	SET 
		UpdatedOn = GETDATE(),
		UpdatedBy = @UpdatedBy
	WHERE 
		CourseRunStatus = @ArchivedStatus
		AND UpdatedOn IS NULL

RETURN 1
