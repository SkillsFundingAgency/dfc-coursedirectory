CREATE TYPE [Pttcd].[VenueUploadRowTable] AS TABLE
(
	[RowNumber] INT NOT NULL,
	[IsValid] BIT NOT NULL,
	[Errors] VARCHAR(MAX),
	[LastUpdated] DATETIME,
	[LastValidated] DATETIME NOT NULL,
	[ProviderVenueRef] NVARCHAR(MAX),
	[VenueName] NVARCHAR(MAX),
	[AddressLine1] NVARCHAR(MAX),
	[AddressLine2] NVARCHAR(MAX),
	[Town] NVARCHAR(MAX),
	[County] NVARCHAR(MAX),
	[Postcode] NVARCHAR(MAX),
	[Email] NVARCHAR(MAX),
	[Telephone] NVARCHAR(MAX),
	[Website] NVARCHAR(MAX),
	[VenueId] UNIQUEIDENTIFIER,
	[OutsideOfEngland] BIT,
	[IsSupplementary] BIT NOT NULL,
	[IsDeletable] BIT NOT NULL
)
