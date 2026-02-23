CREATE TABLE [pttcd].[ProviderUploadRows]
(
	[ProviderUploadRowId] BIGINT IDENTITY CONSTRAINT [PK_ProviderUploadRows] PRIMARY KEY,
	[ProviderUploadId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [FK_ProviderUploads_ProviderUpload] FOREIGN KEY REFERENCES [Pttcd].[ProviderUploads] ([ProviderUploadId]),
	[RowNumber] INT NOT NULL,
	[ProviderUploadRowStatus] TINYINT NOT NULL,
	[IsValid] BIT NOT NULL,
	[Errors] VARCHAR(MAX),
	[LastUpdated] DATETIME NOT NULL,
	[LastValidated] DATETIME NOT NULL,
	[ProviderId] UNIQUEIDENTIFIER NOT NULL,
	[Ukprn] INT NOT NULL,
    [ProviderStatus] INT NULL,
    [ProviderType] INT NULL,
    [ProviderName] NVARCHAR(MAX) NULL,
    [TradingName] NVARCHAR(MAX) NULL, 
    [PIMSOrgStatus] NVARCHAR(50) NULL, 
    [PIMSOrgStatusDate] DATETIME NULL
)
