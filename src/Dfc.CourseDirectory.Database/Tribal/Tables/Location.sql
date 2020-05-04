CREATE TABLE [Tribal].[Location](
	[LocationId] [int] NOT NULL,
	[ProviderId] [int] NOT NULL,
	[ProviderOwnLocationRef] [nvarchar](255) NULL,
	[LocationName] [nvarchar](255) NULL,
	[AddressId] [int] NOT NULL,
	[Telephone] [nvarchar](30) NULL,
	[Email] [nvarchar](255) NULL,
	[Website] [nvarchar](255) NULL,
	[RecordStatusId] [int] NOT NULL,
	[CreatedByUserId] [nvarchar](128) NOT NULL,
	[CreatedDateTimeUtc] [datetime] NOT NULL,
	[ModifiedByUserId] [nvarchar](128) NULL,
	[ModifiedDateTimeUtc] [datetime] NULL,
	[BulkUploadLocationId] [nvarchar](255) NULL
) ON [PRIMARY]
GO


