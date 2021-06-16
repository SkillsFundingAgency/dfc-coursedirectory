CREATE UNIQUE INDEX [IX_CourseUploadRowSubRegions_RowRegion]
	ON [Pttcd].[CourseUploadRowSubRegions]
	([CourseUploadId], [RowNumber], [RegionId])
