CREATE TABLE [Pttcd].[VenueUploads]
(
	[VenueUploadId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_VenueUploads] PRIMARY KEY,
	[ProviderId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [FK_VenueUploads_Provider] FOREIGN KEY REFERENCES [Pttcd].[Providers] ([ProviderId]),
	[UploadStatus] TINYINT NOT NULL CONSTRAINT [DF_VenueUploads_VenueUploadStatus] DEFAULT (0),
	[CreatedOn] DATETIME NOT NULL,
	[CreatedByUserId] VARCHAR(100) NOT NULL CONSTRAINT [FK_VenueUploads_User] FOREIGN KEY REFERENCES [Pttcd].[Users] ([UserId]),
	[ProcessingStartedOn] DATETIME,
	[ProcessingCompletedOn] DATETIME,
	[PublishedOn] DATETIME,
	[AbandonedOn] DATETIME,
	[LastValidated] DATETIME
)
