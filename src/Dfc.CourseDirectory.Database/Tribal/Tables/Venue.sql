CREATE TABLE [Tribal].[Venue](
	[VenueId] [int] NOT NULL,
	[ProviderId] [int] NOT NULL,
	[ProviderOwnVenueRef] [nvarchar](255) NULL,
	[VenueName] [nvarchar](255) NOT NULL,
	[Email] [nvarchar](255) NULL,
	[Website] [nvarchar](255) NULL,
	[Fax] [nvarchar](35) NULL,
	[Facilities] [nvarchar](2000) NULL,
	[RecordStatusId] [int] NOT NULL,
	[CreatedByUserId] [nvarchar](128) NOT NULL,
	[CreatedDateTimeUtc] [datetime] NOT NULL,
	[ModifiedByUserId] [nvarchar](128) NULL,
	[ModifiedDateTimeUtc] [datetime] NULL,
	[AddressId] [int] NOT NULL,
	[Telephone] [nvarchar](30) NULL,
	[BulkUploadVenueId] [nvarchar](255) NULL
) ON [PRIMARY]
GO


