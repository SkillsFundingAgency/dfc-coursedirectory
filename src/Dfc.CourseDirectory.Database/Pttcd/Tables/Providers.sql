﻿CREATE TABLE [Pttcd].[Providers]
(
	[ProviderId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Providers PRIMARY KEY,
	[LastSyncedFromCosmos] DATETIME,
	[Version] ROWVERSION NOT NULL,
	[Ukprn] INT,
	[ProviderStatus] TINYINT,
	[ProviderType] TINYINT,
	[ProviderName] NVARCHAR(MAX),
	[UkrlpProviderStatusDescription] NVARCHAR(MAX),
	[MarketingInformation] NVARCHAR(MAX),
	[CourseDirectoryName] NVARCHAR(MAX),
	[TradingName] NVARCHAR(MAX),
	[Alias] NVARCHAR(MAX),
	[UpdatedOn] DATETIME,
	[UpdatedBy] NVARCHAR(MAX),
	[DisplayNameSource] INT NOT NULL CONSTRAINT [DF_Providers_DisplayNameSource] DEFAULT (0),
	[TribalProviderId] INT,
	[BulkUploadInProgress] BIT NULL,
	[BulkUploadPublishInProgress] BIT NULL,
	[BulkUploadStartedDateTime] DATETIME2 (7) NULL,
	[BulkUploadTotalRowCount] INT NULL
)
