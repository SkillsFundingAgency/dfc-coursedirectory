﻿CREATE TABLE [Pttcd].[Apprenticeships]
(
	[ApprenticeshipId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_Apprenticeships] PRIMARY KEY,
	[ApprenticeshipStatus] INT,
	[CreatedOn] DATETIME,
	[CreatedBy] NVARCHAR(MAX),
	[UpdatedOn] DATETIME,
	[UpdatedBy] NVARCHAR(MAX),
	[TribalApprenticeshipId] INT,
	[ProviderId] UNIQUEIDENTIFIER,
	[ProviderUkprn] INT,
	[ApprenticeshipType] TINYINT,
	[ApprenticeshipTitle] NVARCHAR(MAX),
	[StandardCode] INT,
	[StandardVersion] INT,
	[FrameworkCode] INT,
	[FrameworkProgType] INT,
	[FrameworkPathwayCode] INT,
	[MarketingInformation] NVARCHAR(MAX),
	[ApprenticeshipWebsite] NVARCHAR(MAX),
	[ContactTelephone] NVARCHAR(MAX),
	[ContactEmail] NVARCHAR(MAX),
	[ContactWebsite] NVARCHAR(MAX)
)