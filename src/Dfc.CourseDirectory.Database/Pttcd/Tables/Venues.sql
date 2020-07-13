CREATE TABLE [Pttcd].[Venues]
(
	[VenueId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_Venues] PRIMARY KEY,
	[VenueStatus] TINYINT,
	[CreatedOn] DATETIME,
	[CreatedBy] NVARCHAR(MAX),
	[UpdatedOn] DATETIME,
	[UpdatedBy] NVARCHAR(MAX),
	[VenueName] NVARCHAR(MAX),
	[ProviderUkprn] INT,
	[TribalProviderId] INT,
	[TribalVenueId] INT,
	[ProviderVenueRef] NVARCHAR(MAX),
	[AddressLine1] NVARCHAR(MAX),
	[AddressLine2] NVARCHAR(MAX),
	[Town] NVARCHAR(MAX),
	[County] NVARCHAR(MAX),
	[Postcode] NVARCHAR(MAX),
	[Position] GEOGRAPHY,
	[Telephone] NVARCHAR(MAX),
	[Email] NVARCHAR(MAX),
	[Website] NVARCHAR(MAX)
)
