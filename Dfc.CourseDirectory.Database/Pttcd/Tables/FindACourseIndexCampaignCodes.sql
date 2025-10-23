CREATE TABLE [Pttcd].[FindACourseIndexCampaignCodes]
(
	[FindACourseIndexCampaignCodeId] BIGINT IDENTITY NOT NULL CONSTRAINT [PK_FindACourseIndexCampaignCodes] PRIMARY KEY,
	[ProviderId] UNIQUEIDENTIFIER NOT NULL,
	[LearnAimRef] VARCHAR(50) NOT NULL,
	[CampaignCodesJson] NVARCHAR(MAX) NOT NULL
)
