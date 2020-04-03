CREATE PROCEDURE dfc_GetApprenticeshipQaDetailslViaApprenticeshipId
	@apprenticeshipId int 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	select [ApprenticeshipQAComplianceId]
      ,[ApprenticeshipId]
      ,[CreatedByUserEmail]
      ,[CreatedDateTimeUtc]
      ,[TextQAd]
      ,[DetailsOfUnverifiableClaim]
      ,[DetailsOfComplianceFailure]
      ,[Passed] from tribal.ApprenticeshipQACompliance where ApprenticeshipId = @apprenticeshipId

select qr.ApprenticeshipQAComplianceId, qr.QAComplianceFailureReason from Tribal.ApprenticeshipQAComplianceFailureReason qr
inner join Tribal.ApprenticeshipQACompliance q on q.ApprenticeshipQAComplianceId = qr.ApprenticeshipQAComplianceId
where q.ApprenticeshipId = @apprenticeshipId
END
GO
