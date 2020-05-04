

CREATE PROCEDURE [dbo].[dfc_CourseTransferProviderAuditAdd]
(
	@CourseTransferId int,
	@Ukprn int,
	@CoursesToBeMigrated int,
	@CoursesGoodToBeMigrated int,
	@CoursesGoodToBeMigratedPending int,
	@CoursesGoodToBeMigratedLive int,
	@CoursesNotGoodToBeMigrated int,
	@CoursesLARSless int,
	@MigrationSuccesses int,
	@MigrationFailures int,
	@ProviderReportFileName varchar(255),
	@TimeTaken varchar(50),
	@MigrationNote nvarchar(max)
)

AS
	INSERT INTO Tribal.CourseTransfer_ProviderAudit
		(CourseTransferId, Ukprn, CoursesToBeMigrated, CoursesGoodToBeMigrated, CoursesGoodToBeMigratedPending, CoursesGoodToBeMigratedLive,
		 CoursesNotGoodToBeMigrated, CoursesLARSless, MigrationSuccesses, MigrationFailures, ProviderReportFileName, TimeTaken, MigrationNote)
	VALUES
		(@CourseTransferId, @Ukprn, @CoursesToBeMigrated, @CoursesGoodToBeMigrated, @CoursesGoodToBeMigratedPending, @CoursesGoodToBeMigratedLive,
		 @CoursesNotGoodToBeMigrated, @CoursesLARSless, @MigrationSuccesses, @MigrationFailures, @ProviderReportFileName, @TimeTaken, @MigrationNote)