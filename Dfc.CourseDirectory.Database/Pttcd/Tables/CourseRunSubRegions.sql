CREATE TABLE [Pttcd].[CourseRunSubRegions]
(
	[CourseRunId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [FK_CourseRunSubRegions_CourseRun] FOREIGN KEY REFERENCES [Pttcd].[CourseRuns] ([CourseRunId]),
	[RegionId] VARCHAR(9) NOT NULL,
	CONSTRAINT [PK_CourseRunSubRegions] PRIMARY KEY ([CourseRunId], [RegionId])
)
