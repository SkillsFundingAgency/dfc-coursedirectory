

CREATE PROCEDURE [dbo].[up_VenueListForCsvExport]
	
AS
/*
*	Name:		[up_VenueListForCsvExport]
*	System: 	Stored procedure interface module
*	Description:	List all venues that are live in a format expected for the Csv Export
*
*	Return Values:	0 = No problem detected
*			1 = General database error.
*	Copyright:	(c) Tribal Education Ltd, 2014
*			All rights reserved.
*
*	$Log:  $
*/

-- This procedure creates the C_VENUES.csv file

DECLARE @LiveRecordStatusId INT = (SELECT RecordStatusId FROM RecordStatus RS WHERE RS.IsPublished = 1 AND RS.IsArchived = 0 AND RS.IsDeleted = 0)

SELECT 
	V.ProviderId AS PROVIDER_ID,
	V.VenueId AS VENUE_ID,
	V.VenueName AS VENUE_NAME,
	V.ProviderOwnVenueRef AS PROV_VENUE_ID,
	V.Telephone AS PHONE,
	A.AddressLine1 AS ADDRESS_1,
	A.AddressLine2 AS ADDRESS_2,
	A.Town AS TOWN,
	A.County AS COUNTY,
	A.Postcode AS POSTCODE,
	V.Email AS EMAIL,
	V.Website AS WEBSITE,
	V.Fax AS FAX,
	V.Facilities AS FACILITIES,
    dbo.GetCsvDateTimeString(V.CreatedDateTimeUtc) AS DATE_CREATED,
	dbo.GetCsvDateTimeString(V.ModifiedDateTimeUtc) AS DATE_UPDATE,
	CASE WHEN V.RecordStatusId = @LiveRecordStatusId THEN 'LIVE' ELSE 'DELETED' END AS STATUS, -- We only export live records to CSV so this not strictly necessary but here in case WHERE changes in future
	'NDLPP_' + CAST(ANUModifiedBy.LegacyUserId AS VARCHAR(10)) AS UPDATED_BY,
	'NDLPP_' + CAST(ANUCreatedBy.LegacyUserId AS VARCHAR(10)) AS CREATED_BY,
	'' AS XMIN, '' AS XMAX, '' AS YMIN, '' AS YMAX, -- Around 20 records have these values, we don't have them in the new schema so not included for Venues
	G.Easting AS X_COORD,
	G.Northing AS Y_COORD,
	SUBSTRING(A.Postcode, 0, CHARINDEX(' ', A.Postcode, 1)) AS SEARCH_REGION,
	'NDLPP' AS SYS_DATA_SOURCE,  -- TODO Need to sort out how Ucas data is imported, currently we only have NDLPP data so defaulting to that
	CASE WHEN V.ModifiedDateTimeUtc IS NOT NULL AND V.ModifiedDateTimeUtc <> V.CreatedDateTimeUtc THEN dbo.GetCsvDateTimeString(V.ModifiedDateTimeUtc) ELSE NULL END AS DATE_UPDATED_COPY_OVER,   -- Note we don't have a copy over dates, as we don't copy a denormalised version to another database, 
	CASE WHEN V.ModifiedDateTimeUtc IS NULL OR V.ModifiedDateTimeUtc = V.CreatedDateTimeUtc THEN dbo.GetCsvDateTimeString(V.CreatedDateTimeUtc) ELSE NULL END AS DATE_CREATED_COPY_OVER -- so these dates default to the modified and created dates, i.e. changes are visible immediately
	FROM dbo.Venue V
	INNER JOIN Tribal.Provider P on P.ProviderId=V.ProviderId
	LEFT OUTER JOIN dbo.Address A ON V.AddressId = A.AddressId
	LEFT OUTER JOIN [Identity].AspNetUsers ANUModifiedBy ON V.ModifiedByUserId = ANUModifiedBy.Id
	LEFT OUTER JOIN [Identity].AspNetUsers ANUCreatedBy ON V.CreatedByUserId = ANUCreatedBy.Id
	LEFT OUTER JOIN dbo.GeoLocation G ON A.Postcode = G.Postcode
	WHERE
		V.RecordStatusId = @LiveRecordStatusId
			AND P.RecordStatusId = @LiveRecordStatusId
			AND P.PublishData = 1
	ORDER BY V.ProviderId


IF @@ERROR <> 0
BEGIN
	RETURN 1
END

RETURN 0