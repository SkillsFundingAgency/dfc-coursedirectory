﻿CREATE TABLE [Pttcd].[VenueUploadRows] (
	[VenueUploadRowId] BIGINT IDENTITY CONSTRAINT [PK_VenueUploadRows] PRIMARY KEY,
	[VenueUploadId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [FK_VenueUploads_VenueUpload] FOREIGN KEY REFERENCES [Pttcd].[VenueUploads] ([VenueUploadId]),
	[RowNumber] INT NOT NULL,
	[VenueUploadRowStatus] TINYINT NOT NULL,
	[IsValid] BIT NOT NULL,
	[Errors] VARCHAR(MAX),
	[LastUpdated] DATETIME NOT NULL,
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
	[IsSupplementary] BIT NOT NULL CONSTRAINT [DF_VenueUploadRows_IsSupplementary] DEFAULT (0),
	[IsDeletable] BIT NOT NULL CONSTRAINT [DF_VenueUploadRows_IsDeletable] DEFAULT (0)
)
