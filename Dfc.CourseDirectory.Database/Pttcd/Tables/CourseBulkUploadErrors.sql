CREATE TABLE [Pttcd].[CourseBulkUploadErrors]
(
	[CourseBulkUploadErrorId] BIGINT IDENTITY NOT NULL CONSTRAINT [PK_CourseBulkUploadErrors] PRIMARY KEY,
	[CourseId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [FK_CourseBulkUploadErrors_Course] FOREIGN KEY REFERENCES [Pttcd].[Courses] ([CourseId]),
	[CourseBulkUploadErrorIndex] INT NOT NULL,
	[LineNumber] INT NOT NULL,
	[Header] NVARCHAR(MAX) NOT NULL,
	[Error] NVARCHAR(MAX) NOT NULL,
)
