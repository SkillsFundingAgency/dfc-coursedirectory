CREATE TABLE [Pttcd].[ApprenticeshipUploads](
	[ApprenticeshipUploadId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_ApprenticeshipUploads] PRIMARY KEY,
	[ProviderId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [FK_ApprenticeshipUploads_Provider] FOREIGN KEY REFERENCES [Pttcd].[Providers] ([ProviderId]),
	[UploadStatus] TINYINT NOT NULL,
	[CreatedOn] DATETIME NOT NULL,
	[CreatedByUserId] VARCHAR(100) NOT NULL CONSTRAINT [FK_ApprenticeshipUploads_User] FOREIGN KEY REFERENCES [Pttcd].[Users] ([UserId]),
	[ProcessingStartedOn] DATETIME,
	[ProcessingCompletedOn] DATETIME,
	[PublishedOn] DATETIME,
	[AbandonedOn] DATETIME
)
