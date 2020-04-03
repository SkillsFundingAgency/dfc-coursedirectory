CREATE TABLE [Tribal].[Apprenticeship](
	[ApprenticeshipId] [int] NOT NULL,
	[ProviderId] [int] NOT NULL,
	[CreatedByUserId] [nvarchar](128) NOT NULL,
	[CreatedDateTimeUtc] [datetime] NOT NULL,
	[ModifiedByUserId] [nvarchar](128) NULL,
	[ModifiedDateTimeUtc] [datetime] NULL,
	[AddedByApplicationId] [int] NOT NULL,
	[RecordStatusId] [int] NOT NULL,
	[StandardCode] [int] NULL,
	[Version] [int] NULL,
	[FrameworkCode] [int] NULL,
	[ProgType] [int] NULL,
	[PathwayCode] [int] NULL,
	[MarketingInformation] [nvarchar](900) NULL,
	[Url] [nvarchar](255) NULL,
	[ContactTelephone] [nvarchar](30) NULL,
	[ContactEmail] [nvarchar](255) NULL,
	[ContactWebsite] [nvarchar](255) NULL
) ON [PRIMARY]
GO