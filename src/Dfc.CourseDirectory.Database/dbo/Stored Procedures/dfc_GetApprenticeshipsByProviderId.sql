CREATE PROCEDURE [dbo].[dfc_GetApprenticeshipsByProviderId]
(
	@ProviderId int
)
AS

	SELECT [ApprenticeshipId]
      ,[ProviderId]
      ,[StandardCode]
      ,[Version]
      ,[FrameworkCode]
      ,[ProgType]
      ,[PathwayCode]
      ,[MarketingInformation]
      ,[Url]
      ,[ContactTelephone]
      ,[ContactEmail]
      ,[ContactWebsite]
  FROM [Tribal].[Apprenticeship]
  WHERE RecordStatusId = 2 AND ProviderId = @ProviderId

GO