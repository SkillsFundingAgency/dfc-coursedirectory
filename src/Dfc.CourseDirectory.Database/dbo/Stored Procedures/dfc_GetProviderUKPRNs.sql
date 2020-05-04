


CREATE PROCEDURE [dbo].[dfc_GetProviderUKPRNs]

AS
	SELECT	Ukprn
	FROM	Tribal.CourseMigration
	WHERE	ReadyToMigrate = 1