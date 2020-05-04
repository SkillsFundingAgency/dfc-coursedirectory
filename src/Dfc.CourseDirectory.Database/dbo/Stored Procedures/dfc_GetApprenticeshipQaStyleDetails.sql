CREATE PROCEDURE dfc_GetApprenticeshipQaStyleDetails 
	@apprenticeshipId int 
AS
BEGIN
	SET NOCOUNT ON;

    SELECT [ApprenticeshipQAStyleId]
      ,[ApprenticeshipId]
      ,[CreatedByUserEmail]
      ,[CreatedDateTimeUtc]
      ,[TextQAd]
      ,[Passed]
      ,[DetailsOfQA]
  FROM [Tribal].[ApprenticeshipQAStyle]
  where ApprenticeshipId = @apprenticeshipId

  select qs.ApprenticeshipQAStyleId, qs.QAStyleFailureReason from Tribal.ApprenticeshipQAStyleFailureReason qs
  inner join Tribal.ApprenticeshipQAStyle q on q.ApprenticeshipQAStyleId = qs.ApprenticeshipQAStyleId
  where q.ApprenticeshipId = @apprenticeshipId
END
GO
