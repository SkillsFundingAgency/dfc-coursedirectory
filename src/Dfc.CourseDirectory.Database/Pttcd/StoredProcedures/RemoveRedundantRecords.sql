CREATE PROCEDURE [Pttcd].[RemoveRedundantRecords]
	@RetentionDate datetime
AS
	DECLARE @NonLiveStatus int = 0
	DECLARE @ArchivedStatus int = 4
	DECLARE @UpdatedBy varchar(100) = 'RemoveRedundantRecordsStoredProc'
	
	DELETE 
	FROM 
		Pttcd.FindACourseIndex 
	WHERE 
		Live = @NonLiveStatus 
		AND LastSynced < @RetentionDate

	DELETE 
	FROM 
		Pttcd.CourseRuns 
	WHERE 
		CourseRunStatus = @ArchivedStatus 
		AND UpdatedOn < @RetentionDate

	DELETE 
	FROM 
		Pttcd.Courses 
	WHERE 
		CourseStatus = @ArchivedStatus 
		AND UpdatedOn < @RetentionDate

	DELETE 
	FROM 
		Pttcd.Venues 
	WHERE 
		VenueStatus = @ArchivedStatus 
		AND UpdatedOn < @RetentionDate


	-- Some of the records in tables are archived but updatedon fields are not set. Below statements set UpdatedOn field so that these records can also be deleted.
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

	UPDATE 
		Pttcd.Venues 
	SET 
		UpdatedOn = GETDATE(),
		UpdatedBy = @UpdatedBy
	WHERE 
		VenueStatus = @ArchivedStatus
		AND UpdatedOn IS NULL

RETURN 1
