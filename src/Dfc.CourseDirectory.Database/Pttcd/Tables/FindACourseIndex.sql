﻿CREATE TABLE [Pttcd].[FindACourseIndex] (
	[Id] VARCHAR(46) NOT NULL CONSTRAINT [PK_FindACourseIndex] PRIMARY KEY,
	[LastSynced] DATETIME,
	[OfferingType] INT NOT NULL,
	[CourseId] UNIQUEIDENTIFIER,
	[CourseRunId] UNIQUEIDENTIFIER,
	[TLevelId] UNIQUEIDENTIFIER,
	[TLevelLocationId] UNIQUEIDENTIFIER,
	[Live] BIT NOT NULL,
	[Version] TIMESTAMP NOT NULL,
	[UpdatedOn] DATETIME,
	[ProviderId] UNIQUEIDENTIFIER NOT NULL,
	[ProviderDisplayName] NVARCHAR(MAX) NOT NULL,
	[ProviderUkprn] INT NOT NULL,
	[QualificationCourseTitle] NVARCHAR(MAX),
	[LearnAimRef] VARCHAR(50),
	[NotionalNVQLevelv2] NVARCHAR(MAX),
	[CourseDescription] NVARCHAR(MAX),
	[CourseName] NVARCHAR(MAX),
	[DeliveryMode] TINYINT,
	[FlexibleStartDate] BIT,
	[StartDate] DATE,
	[Cost] MONEY,
	[CostDescription] NVARCHAR(MAX),
	[DurationUnit] TINYINT,
	[DurationValue] INT,
	[StudyMode] TINYINT,
	[AttendancePattern] TINYINT,
	[National] BIT,
	[VenueName] NVARCHAR(MAX),
	[VenueAddress] NVARCHAR(MAX),
	[VenueTown] NVARCHAR(MAX),
	[Position] GEOGRAPHY,
	[RegionName] NVARCHAR(100),
	[ScoreBoost] FLOAT NOT NULL,
	[VenueId] UNIQUEIDENTIFIER,
	[CampaignCodes] NVARCHAR(MAX),
	[CourseDataIsHtmlEncoded] BIT,
	[CourseRunDataIsHtmlEncoded] BIT,
	[CourseType] TINYINT NULL,
	[SectorId] TINYINT NULL,
	[EducationLevel] TINYINT NULL,
	[AwardingBody] NVARCHAR(MAX) NULL
)
