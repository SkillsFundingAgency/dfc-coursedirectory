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
