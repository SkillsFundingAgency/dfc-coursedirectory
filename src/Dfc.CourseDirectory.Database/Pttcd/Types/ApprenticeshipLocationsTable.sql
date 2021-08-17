CREATE TYPE [Pttcd].[ApprenticeshipLocationsTable] AS TABLE
(
	[ApprenticeshipLocationId] UNIQUEIDENTIFIER,
	[Telephone] NVARCHAR(MAX),
	[VenueId] UNIQUEIDENTIFIER,
	[National] BIT,
	[Radius] INT,
	[ApprenticeshipLocationType] TINYINT,
	[DeliveryModes] TINYINT
)
