CREATE TABLE [Tribal].[ApprenticeshipLocation](
	[ApprenticeshipLocationId] [int] NOT NULL,
	[ApprenticeshipId] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
	[Radius] [int] NULL,
	[RecordStatusId] [int] NOT NULL,
	[AddedByApplicationId] [int] NOT NULL,
	[CreatedByUserId] [nvarchar](128) NOT NULL,
	[CreatedDateTimeUtc] [datetime] NOT NULL,
	[ModifiedByUserId] [nvarchar](128) NULL,
	[ModifiedDateTimeUtc] [datetime] NULL
) ON [PRIMARY]
GO