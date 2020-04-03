CREATE PROCEDURE [dbo].[dfc_GetApprenticeshipLocationsDetailsByApprenticeshipId]
(
		@ApprenticeshipId int 
)
AS

 SELECT  al.[ApprenticeshipLocationId]
      ,al.[ApprenticeshipId]
      ,al.[LocationId]
      ,al.[Radius]
	  ,l.LocationName
	  ,a.Postcode
	  ,a.AddressLine1
	  ,a.AddressLine2
	  ,a.Town
	  ,a.Longitude
	  ,a.Latitude
  FROM [Tribal].[ApprenticeshipLocation] al
  INNER JOIN [Tribal].Location l on l.LocationId = al.LocationId
  INNER JOIN [Tribal].Address a on a.AddressId = l.AddressId
  WHERE al.RecordStatusId = 2 AND [ApprenticeshipId] = @ApprenticeshipId

GO