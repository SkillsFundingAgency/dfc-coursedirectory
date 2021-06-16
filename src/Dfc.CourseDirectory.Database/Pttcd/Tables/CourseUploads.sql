CREATE TABLE [Pttcd].[CourseUploads]
(
	[CourseUploadId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_CourseUploads] PRIMARY KEY,
	[ProviderId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [FK_CourseUploads_Provider] FOREIGN KEY REFERENCES [Pttcd].[Providers] ([ProviderId]),
	[UploadStatus] TINYINT NOT NULL,
	[CreatedOn] DATETIME NOT NULL,
	[CreatedByUserId] VARCHAR(100) NOT NULL CONSTRAINT [FK_CourseUploads_User] FOREIGN KEY REFERENCES [Pttcd].[Users] ([UserId]),
	[ProcessingStartedOn] DATETIME,
	[ProcessingCompletedOn] DATETIME,
	[PublishedOn] DATETIME,
	[AbandonedOn] DATETIME
)
