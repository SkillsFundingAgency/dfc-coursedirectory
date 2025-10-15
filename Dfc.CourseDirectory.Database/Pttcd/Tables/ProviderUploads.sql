CREATE TABLE [pttcd].[ProviderUploads]
(
	[ProviderUploadId] UNIQUEIDENTIFIER NOT NULL  CONSTRAINT [PK_ProviderUploads] PRIMARY KEY, 
    [UploadStatus] TINYINT NOT NULL, 
	[CreatedOn] DATETIME NOT NULL,
	[CreatedByUserId] VARCHAR(100) NOT NULL CONSTRAINT [FK_ProviderUploads_User] FOREIGN KEY REFERENCES [Pttcd].[Users] ([UserId]),
	[ProcessingStartedOn] DATETIME,
	[ProcessingCompletedOn] DATETIME,
	[PublishedOn] DATETIME,
	[AbandonedOn] DATETIME,
)
