

CREATE PROCEDURE [dbo].[dfc_CourseTransferUpdate]
(
	@CourseTransferId int,
	@CountProvidersToBeMigrated int,
	@CountProvidersMigrated int,
	@CountProvidersNotMigrated int,
	@CountAllCoursesToBeMigrated int,
	@CountCoursesGoodToBeMigrated int,
	@CountCoursesNotGoodToBeMigrated int,
	@CountCoursesGoodToBeMigratedLive int,
	@CountCoursesGoodToBeMigratedPending int,
	@CountAllCoursesLARSless int,
	@CountAllCoursesMigrated int,
	@CountAllCoursesNotMigrated int,
	@CompleteTransferDateTime datetime,
	@TimeTaken varchar(50),
	@BulkUploadFileName nvarchar(1000),
	@AdminReportFileName varchar(255),
	@TransferNote nvarchar(max)
)

AS
	UPDATE	Tribal.CourseTransfer
	SET		CountProvidersToBeMigrated = @CountProvidersToBeMigrated,
			CountProvidersMigrated = @CountProvidersMigrated,
			CountProvidersNotMigrated = @CountProvidersNotMigrated,
			CountAllCoursesToBeMigrated = @CountAllCoursesToBeMigrated,
			CountCoursesGoodToBeMigrated = @CountCoursesGoodToBeMigrated,
			CountCoursesNotGoodToBeMigrated = @CountCoursesNotGoodToBeMigrated,
			CountCoursesGoodToBeMigratedLive = @CountCoursesGoodToBeMigratedLive,
			CountCoursesGoodToBeMigratedPending = @CountCoursesGoodToBeMigratedPending,
			CountAllCoursesLARSless = @CountAllCoursesLARSless,
			CountAllCoursesMigrated = @CountAllCoursesMigrated,
			CountAllCoursesNotMigrated = @CountAllCoursesNotMigrated,
			CompleteTransferDateTime = @CompleteTransferDateTime,
			TimeTaken = @TimeTaken,
			BulkUploadFileName = @BulkUploadFileName,
			AdminReportFileName = @AdminReportFileName,
			TransferNote = @TransferNote
	WHERE	CourseTransferId = @CourseTransferId