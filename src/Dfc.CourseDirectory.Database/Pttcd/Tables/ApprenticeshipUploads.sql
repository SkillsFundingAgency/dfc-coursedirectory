CREATE TABLE [Pttcd].[ApprenticeshipUploads](
	[ApprenticeshipUploadId] [uniqueidentifier] NOT NULL,
	[ProviderId] [uniqueidentifier] NOT NULL,
	[UploadStatus] [tinyint] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedByUserId] [varchar](100) NOT NULL,
	[ProcessingStartedOn] [datetime] NULL,
	[ProcessingCompletedOn] [datetime] NULL,
	[PublishedOn] [datetime] NULL,
	[AbandonedOn] [datetime] NULL,
	[LastValidated] [datetime] NULL
)
