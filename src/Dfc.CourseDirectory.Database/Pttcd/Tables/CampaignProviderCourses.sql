CREATE TABLE [Pttcd].[CampaignProviderCourses]
(
	[CampaignProviderCourseId] BIGINT IDENTITY NOT NULL CONSTRAINT [PK_CampaignProviderCourses] PRIMARY KEY,
	[CampaignCode] NVARCHAR(100) NOT NULL,
	[ProviderUkprn] INT NOT NULL,
	[LearnAimRef] VARCHAR(50) NOT NULL
)
