CREATE TABLE [Pttcd].[CourseRunBulkUploadErrors]
(
	[CourseRunBulkUploadErrorId] BIGINT IDENTITY NOT NULL CONSTRAINT [PK_CourseRunBulkUploadErrors] PRIMARY KEY,
	[CourseRunId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [FK_CourseRunBulkUploadErrors_Course] FOREIGN KEY REFERENCES [Pttcd].[CourseRuns] ([CourseRunId]),
	[CourseRunBulkUploadErrorIndex] INT NOT NULL,
	[LineNumber] INT NOT NULL,
	[Header] NVARCHAR(MAX) NOT NULL,
	[Error] NVARCHAR(MAX) NOT NULL,
)
