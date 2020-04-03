CREATE TABLE [Pttcd].[ApprenticeshipQASubmissions]
(
	[ApprenticeshipQASubmissionId] INT IDENTITY NOT NULL CONSTRAINT [PK_ApprenticeshipQASubmissions] PRIMARY KEY,
	[ProviderId] UNIQUEIDENTIFIER NOT NULL,
	[SubmittedOn] DATETIME NOT NULL,
	[SubmittedByUserId] VARCHAR(100) NOT NULL CONSTRAINT [FK_ApprenticeshipQASubmissions_SubmittedByUser] FOREIGN KEY REFERENCES [Pttcd].[Users] ([UserId]),
	[ProviderMarketingInformation] NVARCHAR(MAX) NOT NULL,
	[Passed] BIT,
	[LastAssessedByUserId] VARCHAR(100) CONSTRAINT [FK_ApprenticeshipQASubmissions_LastAssessedByUser] FOREIGN KEY REFERENCES [Pttcd].[Users] ([UserId]),
	[LastAssessedOn] DATETIME,
	[ProviderAssessmentPassed] BIT,
	[ApprenticeshipAssessmentsPassed] BIT,
	[HidePassedNotification] BIT NOT NULL CONSTRAINT [DF_ApprenticeshipQASubmissions_HidePassedNotification] DEFAULT (0)
)
