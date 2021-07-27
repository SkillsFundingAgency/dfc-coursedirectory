CREATE TYPE [Pttcd].[CourseRunsTable] AS TABLE
(
	[CourseRunId] UNIQUEIDENTIFIER NOT NULL,
	[CourseName] NVARCHAR(MAX),
	[VenueId] UNIQUEIDENTIFIER,
	[ProviderCourseId] NVARCHAR(MAX),
	[DeliveryMode] TINYINT,
	[FlexibleStartDate] BIT,
	[StartDate] DATE,
	[CourseWebsite] NVARCHAR(MAX),
	[Cost] MONEY,
	[CostDescription]  NVARCHAR(MAX),
	[DurationUnit] TINYINT,
	[DurationValue] INT,
	[StudyMode] TINYINT,
	[AttendancePattern] TINYINT,
	[National] BIT
)
