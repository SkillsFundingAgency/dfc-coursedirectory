CREATE PROCEDURE [Pttcd].[RemoveRedundantRecords]
	@RetentionDate datetime
AS
	DECLARE @NonLiveStatus int = 0
	DECLARE @ArchivedStatus int = 4
	DECLARE @UpdatedBy varchar(100) = 'RemoveRedundantRecordsStoredProc'	


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
		crbue 
	FROM 
		Pttcd.CourseRuns cr
		INNER JOIN Pttcd.CourseRunBulkUploadErrors crbue on crbue.CourseRunId = cr.CourseRunId
	WHERE 
		cr.CourseRunStatus = @ArchivedStatus


	DELETE 		
	FROM 
		Pttcd.CourseRuns 
	WHERE 
		CourseRunStatus = @ArchivedStatus 
		AND UpdatedOn < @RetentionDate
		AND CourseRunId NOT IN (Select Distinct CourseRunId From Pttcd.CourseRunRegions)
		AND CourseRunId NOT IN (Select Distinct CourseRunId From Pttcd.CourseRunSubRegions)

	DELETE 
		cbue 
	FROM 
		Pttcd.Courses c
		INNER JOIN Pttcd.CourseBulkUploadErrors cbue on cbue.CourseId = c.CourseId
	WHERE 
		c.CourseStatus = @ArchivedStatus

	DELETE 		
	FROM 
		Pttcd.Courses 
	WHERE 
		CourseStatus = @ArchivedStatus 
		and UpdatedOn < @RetentionDate
		and CourseId NOT IN (Select Distinct CourseId From Pttcd.CourseRuns)

	DELETE 
		tll
	FROM 
		Pttcd.Venues v
		INNER JOIN Pttcd.TLevelLocations tll ON tll.VenueId = v.VenueId
	WHERE 
		VenueStatus = @ArchivedStatus 
		AND UpdatedOn < @RetentionDate

	DELETE 
	FROM 
		Pttcd.Venues 
	WHERE 
		VenueStatus = @ArchivedStatus 
		AND UpdatedOn < @RetentionDate


	DELETE 		
	FROM 
		Pttcd.FindACourseIndex 
	WHERE 
		Live = @NonLiveStatus 
		AND LastSynced < @RetentionDate

	UPDATE 
		STATISTICS [Pttcd].[FindACourseIndex]  
	WITH
		FULLSCAN

	EXEC sp_recompile N'[Pttcd].[FindACourseIndex]'

RETURN 1
