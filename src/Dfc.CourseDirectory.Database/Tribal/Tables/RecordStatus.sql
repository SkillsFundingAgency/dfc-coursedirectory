CREATE TABLE [Tribal].[RecordStatus](
	[RecordStatusId] [int] NOT NULL,
	[RecordStatusName] [varchar](50) NULL,
	[IsPublished] [bit] NOT NULL,
	[IsArchived] [bit] NOT NULL,
	[IsDeleted] [bit] NOT NULL
) ON [PRIMARY]
GO