CREATE TABLE [Pttcd].[CourseUploadRowSubRegions] (
	[CourseUploadRowSubRegionId] BIGINT IDENTITY CONSTRAINT [PK_CourseUploadRowSubRegions] PRIMARY KEY,
	[CourseUploadId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [FK_CourseUploadRowSubRegions_CourseUploadRow] FOREIGN KEY REFERENCES [Pttcd].[CourseUploads] ([CourseUploadId]),
	[RowNumber] INT NOT NULL,
	[RegionId] VARCHAR(12) NOT NULL CONSTRAINT [FK_CourseUploadRowSubRegions_Region] FOREIGN KEY REFERENCES [Pttcd].[Regions] ([RegionId])
)
