CREATE TABLE [Tribal].[Address](
	[AddressId] [int] NOT NULL,
	[AddressLine1] [nvarchar](110) NULL,
	[AddressLine2] [nvarchar](100) NULL,
	[Town] [nvarchar](75) NULL,
	[County] [nvarchar](75) NULL,
	[Postcode] [nvarchar](30) NOT NULL,
	[ProviderRegionId] [int] NULL,
	[Latitude] [float] NULL,
	[Longitude] [float] NULL,
	[Geography] [geography] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
