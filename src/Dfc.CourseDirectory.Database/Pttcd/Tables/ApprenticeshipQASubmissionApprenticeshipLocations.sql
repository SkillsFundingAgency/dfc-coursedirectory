CREATE TABLE [Pttcd].[ApprenticeshipQASubmissionApprenticeshipLocations]
(
	[ApprenticeshipQASubmissionApprenticeshipLocationId] INT IDENTITY NOT NULL CONSTRAINT [PK_ApprenticeshipQASubmissionApprenticeshipLocations] PRIMARY KEY,
	[ApprenticeshipQASubmissionApprenticeshipId] INT NOT NULL CONSTRAINT [FK_ApprenticeshipQASubmissionApprenticeshipLocations_ApprenticeshipQASubmissionApprenticeship] FOREIGN KEY REFERENCES [Pttcd].[ApprenticeshipQASubmissionApprenticeships] ([ApprenticeshipQASubmissionApprenticeshipId]),
	[ApprenticeshipLocationType] TINYINT NOT NULL,
	[National] BIT,
	[RegionIds] VARCHAR(MAX),
	[VenueName] NVARCHAR(MAX),
	[DeliveryModes] VARCHAR(MAX),
	[Radius] INT,
)
