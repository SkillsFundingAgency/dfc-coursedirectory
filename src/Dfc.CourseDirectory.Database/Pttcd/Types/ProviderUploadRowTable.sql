CREATE TYPE [Pttcd].[ProviderUploadRowTable] AS TABLE
(
	[RowNumber] INT NOT NULL,
	[IsValid] BIT NOT NULL,
	[Errors] VARCHAR(MAX),
	[LastUpdated] DATETIME,
	[LastValidated] DATETIME NOT NULL,
	[ProviderId] UNIQUEIDENTIFIER,
	[UKprn] INT,
	[ProviderStatus] TINYINT,
	[ProviderType] TINYINT,
	[ProviderName] NVARCHAR(MAX),
	[TradingName] NVARCHAR(MAX)
)
