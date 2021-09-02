CREATE TABLE [Pttcd].[ApprenticeshipUploadRowSubRegions] (
	[ApprenticeshipUploadRowSubRegionId] BIGINT IDENTITY CONSTRAINT [PK_ApprenticeshipUploadRowSubRegions] PRIMARY KEY,
	[ApprenticeshipUploadId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [FK_ApprenticeshipUploadRowSubRegions_CourseUploadRow] FOREIGN KEY REFERENCES [Pttcd].[ApprenticeshipUploads] ([ApprenticeshipUploadId]),
	[RowNumber] INT NOT NULL,
	[RegionId] VARCHAR(12) NOT NULL CONSTRAINT [FK_ApprenticeshipUploadRowSubRegions_Region] FOREIGN KEY REFERENCES [Pttcd].[Regions] ([RegionId])
)
