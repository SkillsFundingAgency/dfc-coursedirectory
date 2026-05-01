--------- This script updates CourseType column value in Pttcd.Courses table for already existing courses. It derives CourseType from the mapping inserted in above table i.e. Pttcd.CourseTypeCategory -----------------
SELECT ldc.LearnAimRef,
CASE 
	WHEN ldc.CategoryRef IN (24, 29, 39, 42, 45, 46, 48, 49, 56, 55, 63) THEN ctc.CourseType
	WHEN ldc.CategoryRef = 40 AND (CHARINDEX('ESOL', ld.LearnAimRefTitle COLLATE Latin1_General_CS_AS) > 0 OR CHARINDEX('GCSE (9-1) in English Language', ld.LearnAimRefTitle) > 0 OR CHARINDEX('GCSE (9-1) in English Literature', ld.LearnAimRefTitle) > 0) THEN ctc.CourseType
	WHEN ldc.CategoryRef = 37 AND (CHARINDEX('GCSE (9-1) in English Language', ld.LearnAimRefTitle) > 0 OR CHARINDEX('GCSE (9-1) in English Literature', ld.LearnAimRefTitle) > 0) THEN ctc.CourseType 
	WHEN ldc.CategoryRef = 3 AND LEFT(ld.LearnAimRefTitle, 7) = 'T Level' THEN ctc.CourseType
	ELSE NULL 
END CourseType

INTO #LearnAimRefCourseTypes

FROM LARS.LearningDeliveryCategory ldc
LEFT JOIN Pttcd.CourseTypeCategory ctc ON ctc.CategoryRef = ldc.CategoryRef
INNER JOIN LARS.LearningDelivery ld ON ld.LearnAimRef = ldc.LearnAimRef

UPDATE 
	c
SET 
	c.CourseType = larct.CourseType
FROM 
	Pttcd.Courses c 
	INNER JOIN #LearnAimRefCourseTypes larct ON larct.LearnAimRef = c.LearnAimRef
WHERE 
	c.CourseStatus = 1 
	AND c.CourseType IS NULL
	AND larct.CourseType IS NOT NULL

DROP TABLE #LearnAimRefCourseTypes
-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

---------Update Course type to null for those courses which have course type of 4 but are not in eligible proivders list---------
UPDATE 
	Pttcd.Courses 
SET 
	CourseType = NULL
WHERE 
	CourseType = 4  
	AND CourseStatus = 1
	AND ProviderId NOT IN (SELECT DISTINCT ProviderId FROM Pttcd.FindACourseIndexCampaignCodes WHERE CampaignCodesJson LIKE '%LEVEL3_FREE%')

UPDATE 
	Pttcd.FindACourseIndex 
SET 
	CourseType = NULL
WHERE 
	CourseType = 4  
	AND Live = 1
	AND ProviderId NOT IN (SELECT DISTINCT ProviderId FROM Pttcd.FindACourseIndexCampaignCodes WHERE CampaignCodesJson LIKE '%LEVEL3_FREE%')

---------------------------------------------------------------------------------------------------------------------------------
--AD-254683: Fix incorrectly archived courses
EXEC [Pttcd].[UpdateIncorrectlyArchivedCourses]

--- BUG 254770 : APPLY A FIX TO SET TLevelLocationStatus to Deleted for all TLevelLocations where the parent TLevel is marked as Deleted. 
--- This is required to fix an issue where some TLevelLocations were not marked as Deleted when their parent TLevel was marked as Deleted.

BEGIN TRANSACTION
BEGIN TRY
	UPDATE tll SET
    tll.TLevelLocationStatus = 2
	FROM        Pttcd.TLevelLocations tll
	INNER JOIN  Pttcd.TLevels t ON tll.TLevelId = t.TLevelId
	INNER JOIN  Pttcd.Venues v ON tll.VenueId = v.VenueId
	WHERE       t.TLevelStatus = 2
	AND         TLevelLocationStatus = 1

	COMMIT;
END TRY

BEGIN CATCH
	ROLLBACK;
END CATCH

---------------------------------------------------------------------------------------------------------------------------------------------
-- BUG 262683 : Fix broken course uploads which are stuck in 'NotStarted' status for more than 24 hours by setting their status to 'Failed'.
-- this will fix the uploads which are stuck on Created state (0) for more than 24 hours by setting their status to Abandoned (5)
BEGIN TRANSACTION
BEGIN TRY 
	
	CREATE TABLE #BROKEN_UPLOADS ([CourseUploadId] UNIQUEIDENTIFIER,
							  [ProviderId] UNIQUEIDENTIFIER,
							  [UploadStatus] TINYINT,
							  [CreatedOn] DATETIME);

	INSERT INTO #BROKEN_UPLOADS 
	SELECT [CourseUploadId]
		  ,[ProviderId]
		  ,[UploadStatus]
		  ,[CreatedOn]
	  FROM [Pttcd].[CourseUploads]
	  WHERE UploadStatus = 0 AND CreatedOn < DATEADD(day,-1,GETDATE())

	UPDATE CU
	  SET CU.UploadStatus = 5 ,
		CU.AbandonedOn = GETDATE()
	  FROM [Pttcd].[CourseUploads] CU 
	  JOIN #BROKEN_UPLOADS BU ON CU.CourseUploadId = BU.CourseUploadId AND CU.ProviderId = BU.ProviderId

	DROP TABLE #BROKEN_UPLOADS;
	COMMIT;
END TRY
	
BEGIN CATCH
	ROLLBACK;
END CATCH



---------------------------------------------------------------------------------------------------------------------------------
-- USER STORY 253961 : APPLY A FIX TO REMOVE HTML TAGS FROM COURSE DATA FIELDS FOR COURSES 
-- WHICH HAVE BEEN UPDATED IN LAST 15 MONTHS AND HAVE HTML TAGS IN ANY OF THE COURSE DATA FIELDS.

-- STEP 1: STORE COURSES WITH HTML TAG DATA IN A TEMPORARY TABLE 
SELECT [CourseId]
      ,[ProviderId]
      ,[CreatedOn]
      ,[UpdatedOn]
      ,[LearnAimRef]
      ,[ProviderUkprn]
      ,[CourseDescription]
      ,[EntryRequirements]
      ,[WhatYoullLearn]
      ,[HowYoullLearn]
      ,[WhatYoullNeed]
      ,[HowYoullBeAssessed]
      ,[WhereNext]
   INTO #COURSES_WITH_HTML_TAGS
   FROM [Pttcd].[Courses] WHERE NOT (CourseDescription Is null and EntryRequirements is null and WhatYoullLearn is null and HowYoullLearn is null and WhatYoullNeed is null and HowYoullBeAssessed is null and WhereNext IS null)
        and CourseDescription Like '%<%>%'
        or EntryRequirements Like '%<%>%'
        or WhatYoullLearn Like '%<%>%'
        or HowYoullLearn Like '%<%>%'
        or WhatYoullNeed Like '%<%>%'
        or HowYoullBeAssessed Like '%<%>%'
        or WhereNext Like '%<%>%'
        and CourseStatus = 1
        and UpdatedOn >= DATEADD(m, -15, GETDATE())

-- STEP 2: IF THERE ARE ANY COURSES WITH HTML TAG DATA THEN CREATE A TEMP TABLE TO SAVE DATA TEMPORARILY 
-- AND THEN UPDATE THE COURSE DATA BY REMOVING HTML TAGS ON DATA FILEDS
-- OTHERWISE, IF THERE IS NO COURSE WITH HTML TAG DATA THEN DROP THE TEMP TABLE IF IT EXISTS
IF EXISTS(SELECT CourseId FROM  #COURSES_WITH_HTML_TAGS) 
BEGIN

	-- CREATE A TEMP TABLE TO STORE COURSES WITH HTML TAG DATA IF NOT EXISTS
	IF NOT EXISTS(SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'temp_courses')
	BEGIN
		CREATE TABLE Pttcd.temp_courses (
			[CourseId] [uniqueidentifier] NOT NULL,
			[ProviderId] [uniqueidentifier] NULL,
			[CreatedOn] [datetime] NULL,
			[UpdatedOn] [datetime] NULL,
			[LearnAimRef] [varchar](50) NULL,
			[ProviderUkprn] [int] NULL,
			[CourseDescription] [nvarchar](max) NULL,
			[EntryRequirements] [nvarchar](max) NULL,
			[WhatYoullLearn] [nvarchar](max) NULL,
			[HowYoullLearn] [nvarchar](max) NULL,
			[WhatYoullNeed] [nvarchar](max) NULL,
			[HowYoullBeAssessed] [nvarchar](max) NULL,
			[WhereNext] [nvarchar](max) NULL,
			 CONSTRAINT [PK_temp_Courses] PRIMARY KEY CLUSTERED 
			(
				[CourseId] ASC
			)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
			) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
	END
	-- TRUNCATE THE TEMP TABLE TO REMOVE ANY EXISTING DATA BEFORE INSERTING THE COURSE DATA WITH HTML TAGS 
	-- INTO THIS TEMP TABLE TO COMPARE THE COURSE DATA BEFORE AND AFTER UPDATION
	TRUNCATE TABLE Pttcd.temp_courses;

	-- INSERT THE COURSE DATA WITH HTML TAGS INTO TEMP TABLE TO COMPARE THE COURSE DATA BEFORE AND AFTER UPDATION 
	-- TO ENSURE THAT ONLY HTML TAGS ARE REMOVED AND THERE IS NO OTHER CHANGE IN THE COURSE DATA FIELDS AFTER UPDATION
	INSERT INTO  Pttcd.temp_courses 
	SELECT [CourseId]
      ,[ProviderId]
      ,[CreatedOn]
      ,[UpdatedOn]
      ,[LearnAimRef]
      ,[ProviderUkprn]
      ,[CourseDescription]
      ,[EntryRequirements]
      ,[WhatYoullLearn]
      ,[HowYoullLearn]
      ,[WhatYoullNeed]
      ,[HowYoullBeAssessed]
      ,[WhereNext]
    FROM #COURSES_WITH_HTML_TAGS

	-- UPDATE THE COURSE DATA FIELDS BY REMOVING HTML TAGS FOR COURSES WHICH HAVE BEEN UPDATED IN LAST 15 MONTHS 
	-- AND HAVE HTML TAGS IN ANY OF THE COURSE DATA FIELDS
	BEGIN TRY
		BEGIN TRANSACTION
				UPDATE [Pttcd].[Courses]
					SET CourseDescription = REPLACE(CourseDescription, CourseDescription, [Pttcd].[udf_RemoveHTMLTags]([CourseDescription])),
					EntryRequirements = REPLACE(EntryRequirements, EntryRequirements, [Pttcd].[udf_RemoveHTMLTags]([EntryRequirements])),
					WhatYoullLearn = REPLACE(WhatYoullLearn, WhatYoullLearn, [Pttcd].[udf_RemoveHTMLTags]([WhatYoullLearn])),
					HowYoullLearn = REPLACE(HowYoullLearn, HowYoullLearn, [Pttcd].[udf_RemoveHTMLTags]([HowYoullLearn])),
					WhatYoullNeed = REPLACE(WhatYoullNeed, WhatYoullNeed, [Pttcd].[udf_RemoveHTMLTags]([WhatYoullNeed])),
					HowYoullBeAssessed = REPLACE(HowYoullBeAssessed, HowYoullBeAssessed, [Pttcd].[udf_RemoveHTMLTags]([HowYoullBeAssessed])),
					WhereNext = REPLACE(WhereNext, WhereNext, [Pttcd].[udf_RemoveHTMLTags]([WhereNext])),
					UpdatedOn = GETDATE(),
					UpdatedBy = 'Post Deployment Script - USER STORY 253961'
				WHERE CourseId IN (SELECT CourseId FROM Pttcd.temp_courses)
		COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
	  ROLLBACK TRANSACTION
	END CATCH	
END
-- STEP 3: DROP THE TEMP TABLE WHICH STORES COURSES WITH HTML TAG DATA
	DROP TABLE #COURSES_WITH_HTML_TAGS;
---------------------------------------------------------------------------------------------------------------------------------

