﻿CREATE TABLE [Pttcd].[CourseUploadRows] (
	[CourseUploadRowId] BIGINT IDENTITY CONSTRAINT [PK_CourseUploadRows] PRIMARY KEY,
	[CourseUploadId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [FK_CourseUploads_VenueUpload] FOREIGN KEY REFERENCES [Pttcd].[CourseUploads] ([CourseUploadId]),
	[RowNumber] INT NOT NULL,
	[CourseUploadRowStatus] TINYINT NOT NULL,
	[IsValid] BIT NOT NULL,
	[Errors] VARCHAR(MAX),
	[LastUpdated] DATETIME NOT NULL,
	[LastValidated] DATETIME NOT NULL,
	[CourseId] UNIQUEIDENTIFIER NOT NULL,
	[CourseRunId] UNIQUEIDENTIFIER NOT NULL,
	[LearnAimRef] NVARCHAR(MAX),
	[WhoThisCourseIsFor] NVARCHAR(MAX),
	[EntryRequirements] NVARCHAR(MAX),
	[WhatYouWillLearn] NVARCHAR(MAX),
	[HowYouWillLearn] NVARCHAR(MAX),
	[WhatYouWillNeedToBring] NVARCHAR(MAX),
	[HowYouWillBeAssessed] NVARCHAR(MAX),
	[WhereNext] NVARCHAR(MAX),
	[CourseName] NVARCHAR(MAX),
	[ProviderCourseRef] NVARCHAR(MAX),
	[DeliveryMode] NVARCHAR(MAX),
	[StartDate] NVARCHAR(MAX),
	[FlexibleStartDate] NVARCHAR(MAX),
	[VenueName] NVARCHAR(MAX),
	[ProviderVenueRef] NVARCHAR(MAX),
	[NationalDelivery] NVARCHAR(MAX),
	[SubRegions] NVARCHAR(MAX),
	[CourseWebpage] NVARCHAR(MAX),
	[Cost] NVARCHAR(MAX),
	[CostDescription] NVARCHAR(MAX),
	[Duration] NVARCHAR(MAX),
	[DurationUnit] NVARCHAR(MAX),
	[StudyMode] NVARCHAR(MAX),
	[AttendancePattern] NVARCHAR(MAX),
	[VenueId] UNIQUEIDENTIFIER CONSTRAINT [FK_CourseUploadRows_Venue] FOREIGN KEY REFERENCES [Pttcd].[Venues] ([VenueId]),
	[ResolvedDeliveryMode] TINYINT,
	[ResolvedStartDate] DATE,
	[ResolvedFlexibleStartDate] BIT,
	[ResolvedNationalDelivery] BIT,
	[ResolvedCost] MONEY,
	[ResolvedDuration] INT,
	[ResolvedDurationUnit] TINYINT,
	[ResolvedStudyMode] TINYINT,
	[ResolvedAttendancePattern] TINYINT, 
    [CourseType] NVARCHAR(MAX) NULL,
	[ResolvedCourseType] TINYINT NULL,
	[EducationLevel] NVARCHAR(MAX) NULL,
	[ResolvedEducationLevel] TINYINT NULL,
	[AwardingBody] NVARCHAR(MAX) NULL
)
