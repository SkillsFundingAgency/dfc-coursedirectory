

CREATE PROCEDURE [dbo].[up_UKRLP_Course_Directory_Migration]

AS

DECLARE @venues TABLE(PROVIDER_ID int, VENUE_ID int, VENUE_NAME nvarchar(255), PROV_VENUE_ID nvarchar(255), PHONE nvarchar(30),
					  ADDRESS_1 nvarchar(110), ADDRESS_2 nvarchar(100), TOWN nvarchar(75), COUNTY nvarchar(75), POSTCODE nvarchar(30),
					  EMAIL nvarchar(256), WEBSITE nvarchar(255), FAX nvarchar(35), FACILITIES nvarchar(2000),
					  DATE_CREATED nvarchar(20), DATE_UPDATE nvarchar(20), STATUS nvarchar(20), UPDATED_BY nvarchar(20), CREATED_BY nvarchar(20),
					  XMIN nvarchar(20), XMAX nvarchar(20), YMIN nvarchar(20), YMAX nvarchar(20), X_COORD float,Y_COORD float,
					  SEARCH_REGION nvarchar(30), SYS_DATA_SOURCE nvarchar(20), DATE_UPDATED_COPY_OVER nvarchar(20), DATE_CREATED_COPY_OVER nvarchar(20))
INSERT INTO @venues
	EXEC up_VenueListForCsvExport;

WITH CTE (PROVIDER_ID, VENUE_ID, VENUE_NAME, PROV_VENUE_ID, ADDRESS_1, ADDRESS_2, TOWN, COUNTY, POSTCODE, DATE_UPDATE, UPDATED_BY)
AS (
	--exec up_VenueListForCsvExport
	SELECT PROVIDER_ID, VENUE_ID, VENUE_NAME, PROV_VENUE_ID, ADDRESS_1, ADDRESS_2, TOWN, COUNTY, POSTCODE, DATE_UPDATE, UPDATED_BY
	FROM @venues
)
SELECT		p.Ukprn AS UKPRN, c.*
FROM		Provider p
INNER JOIN	CTE c
ON			p.ProviderId = c.PROVIDER_ID