

CREATE PROCEDURE [dbo].[dfc_CourseTransferCourseAuditAdd]
(
	@CourseTransferId int,
	@Ukprn int,
	@CourseId int,
	@LARS varchar(10),
	@CourseRecordStatus int,
	@CourseRuns int,
	@CourseRunsLive int,
	@CourseRunsPending int,
	@CourseRunsReadyToGoLive int,
	@CourseRunsLARSless int,
	@MigrationSuccess int,
	@CourseMigrationNote nvarchar(max)
)

AS

INSERT INTO Tribal.CourseTransfer_CourseAudit
			(CourseTransferId, Ukprn, CourseId, LARS, CourseRecordStatus, CourseRuns, CourseRunsLive, CourseRunsPending,
			 CourseRunsReadyToGoLive, CourseRunsLARSless, MigrationSuccess, CourseMigrationNote)
	VALUES	(@CourseTransferId, @Ukprn, @CourseId, @LARS, @CourseRecordStatus, @CourseRuns, @CourseRunsLive, @CourseRunsPending,
			 @CourseRunsReadyToGoLive, @CourseRunsLARSless, @MigrationSuccess, @CourseMigrationNote)