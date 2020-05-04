CREATE TABLE [Pttcd].[ApprenticeshipQASubmissionApprenticeships]
(
	[ApprenticeshipQASubmissionApprenticeshipId] INT IDENTITY NOT NULL CONSTRAINT [PK_ApprenticeshipQASubmissionApprenticeships] PRIMARY KEY,
	[ApprenticeshipQASubmissionId] INT NOT NULL CONSTRAINT [FK_ApprenticeshipQASubmissionApprenticeships_QASubmission] FOREIGN KEY REFERENCES [Pttcd].[ApprenticeshipQASubmissions] ([ApprenticeshipQASubmissionId]),
	[ApprenticeshipId] UNIQUEIDENTIFIER NOT NULL,
	[ApprenticeshipTitle] NVARCHAR(MAX) NOT NULL,
	[ApprenticeshipMarketingInformation] NVARCHAR(MAX) NOT NULL
)
