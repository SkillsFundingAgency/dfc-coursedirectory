/*
 Pre-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be executed before the build script.	
 Use SQLCMD syntax to include a file in the pre-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the pre-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
-- AD-255086: ROLLBACK HTML TAGS REMOVED FROM COURSE DATA FIELDS SUCH AS COURSE DESCRIPTION, ENTRY REQUIREMENTS, 
-- WHAT YOU'LL LEARN, HOW YOU'LL LEARN, WHAT YOU'LL NEED, HOW YOU'LL BE ASSESSED AND WHERE NEXT FIELDS IN PTTCD.COURSES TABLE

BEGIN TRY
	IF EXISTS(SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'temp_courses')
	BEGIN
		BEGIN TRANSACTION

		UPDATE C
		   SET C.[CourseDescription] = TC.CourseDescription
			  ,C.[EntryRequirements] = TC.EntryRequirements
			  ,C.[WhatYoullLearn] = TC.WhatYoullLearn
			  ,C.[HowYoullLearn] = TC.HowYoullLearn
			  ,C.[WhatYoullNeed] = TC.WhatYoullNeed
			  ,C.[HowYoullBeAssessed] = TC.HowYoullBeAssessed
			  ,C.[WhereNext] = TC.WhereNext
			  ,C.UpdatedOn = GETUTCDATE()
			  ,C.UpdatedBy = 'PreDeploymentScript - Rollback Release'
		FROM [Pttcd].[Courses] AS C
		JOIN [Pttcd].temp_courses AS TC ON C.CourseId = TC.CourseId AND C.ProviderId =  TC.ProviderId

		DROP TABLE Pttcd.temp_courses;

		COMMIT TRANSACTION;
	END
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION;
END CATCH
