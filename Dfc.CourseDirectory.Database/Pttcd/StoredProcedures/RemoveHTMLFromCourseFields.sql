CREATE PROCEDURE [Pttcd].[usp_RemoveHTMLFromCourseFields]
AS
BEGIN

BEGIN TRY
BEGIN TRANSACTION
        UPDATE [Pttcd].[Courses]
            SET CourseDescription = REPLACE(CourseDescription, CourseDescription, [Pttcd].[udf_RemoveHTMLTags]([CourseDescription])),
            EntryRequirements = REPLACE(EntryRequirements, EntryRequirements, [Pttcd].[udf_RemoveHTMLTags]([EntryRequirements])),
            WhatYoullLearn = REPLACE(WhatYoullLearn, WhatYoullLearn, [Pttcd].[udf_RemoveHTMLTags]([WhatYoullLearn])),
            HowYoullLearn = REPLACE(HowYoullLearn, HowYoullLearn, [Pttcd].[udf_RemoveHTMLTags]([HowYoullLearn])),
            WhatYoullNeed = REPLACE(WhatYoullNeed, WhatYoullNeed, [Pttcd].[udf_RemoveHTMLTags]([WhatYoullNeed])),
            HowYoullBeAssessed = REPLACE(HowYoullBeAssessed, HowYoullBeAssessed, [Pttcd].[udf_RemoveHTMLTags]([HowYoullBeAssessed])),
            WhereNext = REPLACE(WhereNext, WhereNext, [Pttcd].[udf_RemoveHTMLTags]([WhereNext]))
        WHERE NOT (CourseDescription Is null and EntryRequirements is null and WhatYoullLearn is null and HowYoullLearn is null and WhatYoullNeed is null and HowYoullBeAssessed is null and WhereNext IS null)
        and CourseDescription Like '%<%'
        or EntryRequirements Like '%<%'
        or WhatYoullLearn Like '%<%'
        or HowYoullLearn Like '%<%'
        or WhatYoullNeed Like '%<%'
        or HowYoullBeAssessed Like '%<%'
        or WhereNext Like '%<%'
        and CourseStatus = 1
        and UpdatedOn >= DATEADD(m, -15, GETDATE())
COMMIT TRANSACTION
END TRY
BEGIN CATCH
  rollback transaction
END CATCH

END
