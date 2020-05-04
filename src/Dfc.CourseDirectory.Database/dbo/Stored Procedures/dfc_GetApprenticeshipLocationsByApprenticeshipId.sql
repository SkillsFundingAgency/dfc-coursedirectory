CREATE PROCEDURE [dbo].[dfc_GetApprenticeshipLocationsByApprenticeshipId]
(
		@ApprenticeshipId int 
)
AS

 SELECT  [ApprenticeshipLocationId]
      ,[ApprenticeshipId]
      ,[LocationId]
      ,[Radius]
  FROM [Tribal].[ApprenticeshipLocation]
  WHERE RecordStatusId = 2 AND [ApprenticeshipId] = @ApprenticeshipId

GO