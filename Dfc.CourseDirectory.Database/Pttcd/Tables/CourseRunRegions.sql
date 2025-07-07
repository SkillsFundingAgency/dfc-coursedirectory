CREATE TABLE [Pttcd].[CourseRunRegions]
(
	[CourseRunId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [FK_CourseRunRegions_CourseRun] FOREIGN KEY REFERENCES [Pttcd].[CourseRuns] ([CourseRunId]),
	[RegionId] VARCHAR(9) NOT NULL,
	CONSTRAINT [PK_CourseRunRegions] PRIMARY KEY ([CourseRunId], [RegionId])
)
