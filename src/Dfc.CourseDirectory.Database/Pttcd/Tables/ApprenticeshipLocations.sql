CREATE TABLE [Pttcd].[ApprenticeshipLocations]
(
	[ApprenticeshipLocationId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_ApprenticeshipLocations] PRIMARY KEY,
	[ApprenticeshipId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [FK_ApprenticeshipLocations_Apprenticeship] FOREIGN KEY REFERENCES [Pttcd].[Apprenticeships] ([ApprenticeshipId]),
	[ApprenticeshipLocationStatus] TINYINT,
	[CreatedOn] DATETIME,
	[CreatedBy] NVARCHAR(MAX),
	[UpdatedOn] DATETIME,
	[UpdatedBy] NVARCHAR(MAX),
	[Telephone] NVARCHAR(MAX),
	[VenueId] UNIQUEIDENTIFIER,
	[TribalApprenticeshipLocationId] INT,
	[National] BIT,
	[Radius] INT,
	[LocationType] TINYINT,
	[ApprenticeshipLocationType] TINYINT,
	[Name] NVARCHAR(MAX),
	[DeliveryModes] TINYINT
)
