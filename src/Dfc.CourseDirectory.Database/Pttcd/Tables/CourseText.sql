CREATE TABLE [Pttcd].[CourseText]
(
	[LearnAimRef]					NVARCHAR (10)  NOT NULL CONSTRAINT [PK_CourseText] PRIMARY KEY,
	[AwardOrgCode]					NVARCHAR (30)  NULL,
	[QualificationCourseTitle]		NVARCHAR (MAX),
	[NotionalNVQLevelv2]			NVARCHAR (MAX),
	[TypeName]						NVARCHAR (MAX),
	[CourseDescription]				NVARCHAR (MAX),
	[EntryRequirements]				NVARCHAR (MAX),
	[WhatYoullLearn]				NVARCHAR (MAX),
	[HowYoullLearn]					NVARCHAR (MAX),
	[WhatYoullNeed]					NVARCHAR (MAX),
	[HowYoullBeAssessed]			NVARCHAR (MAX),
	[WhereNext]						NVARCHAR (MAX), 
    CONSTRAINT [FK_CourseText_LearningDelivery_LearnAimRef] FOREIGN KEY ([LearnAimRef]) REFERENCES [LARS].[LearningDelivery]([LearnAimRef])





)
