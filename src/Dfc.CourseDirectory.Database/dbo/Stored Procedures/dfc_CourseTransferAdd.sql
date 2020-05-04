

CREATE PROCEDURE [dbo].[dfc_CourseTransferAdd]
(
	@StartTransferDateTime datetime,
    @TransferMethod int,
    @DeploymentEnvironment int,
    @CreatedById nvarchar(128),
    @CreatedByName nvarchar(255),
    @Ukprn int,
	@CourseTransferId int OUTPUT
)

AS
	INSERT INTO Tribal.CourseTransfer (StartTransferDateTime, TransferMethod, DeploymentEnvironment, CreatedById, CreatedByName, Ukprn)
	VALUES (@StartTransferDateTime, @TransferMethod, @DeploymentEnvironment, @CreatedById, @CreatedByName, @Ukprn)
	SELECT @CourseTransferId = SCOPE_IDENTITY()