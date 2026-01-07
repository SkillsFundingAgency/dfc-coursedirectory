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

-- THIS IS ONE OFF SCRIPT TO REMOVE HTML TAGS FROM COURSE DESCRIPTION AND OTHER FIELDS IN PTTCD.COURSES TABLE ----

IF EXISTS(SELECT CourseId FROM Pttcd.Courses  WHERE NOT (CourseDescription Is null and EntryRequirements is null and WhatYoullLearn is null and HowYoullLearn is null and WhatYoullNeed is null and HowYoullBeAssessed is null and WhereNext IS null)
        and CourseDescription Like '%<%'
        or EntryRequirements Like '%<%'
        or WhatYoullLearn Like '%<%'
        or HowYoullLearn Like '%<%'
        or WhatYoullNeed Like '%<%'
        or HowYoullBeAssessed Like '%<%'
        or WhereNext Like '%<%'
        and CourseStatus = 1
        and UpdatedOn >= DATEADD(m, -15, GETDATE())) 
BEGIN
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

	TRUNCATE TABLE Pttcd.temp_courses;

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
   FROM [Pttcd].[Courses] WHERE NOT (CourseDescription Is null and EntryRequirements is null and WhatYoullLearn is null and HowYoullLearn is null and WhatYoullNeed is null and HowYoullBeAssessed is null and WhereNext IS null)
        and CourseDescription Like '%<%'
        or EntryRequirements Like '%<%'
        or WhatYoullLearn Like '%<%'
        or HowYoullLearn Like '%<%'
        or WhatYoullNeed Like '%<%'
        or HowYoullBeAssessed Like '%<%'
        or WhereNext Like '%<%'
        and CourseStatus = 1
        and UpdatedOn >= DATEADD(m, -15, GETDATE())
	EXEC [Pttcd].[usp_RemoveHTMLFromCourseFields]
END
ELSE
BEGIN
	IF EXISTS(SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'temp_courses')
	BEGIN
		DROP TABLE Pttcd.temp_courses;
	END	
END
