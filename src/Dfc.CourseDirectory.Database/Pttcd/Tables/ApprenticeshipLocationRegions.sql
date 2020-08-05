CREATE TABLE [Pttcd].[ApprenticeshipLocationRegions]
(
	[ApprenticeshipLocationId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [FK_ApprenticeshipLocationRegions_ApprenticeshipLocation] FOREIGN KEY REFERENCES [Pttcd].[ApprenticeshipLocations] ([ApprenticeshipLocationId]),
	[RegionId] VARCHAR(9) NOT NULL,
	CONSTRAINT [PK_ApprenticeshipLocationRegion] PRIMARY KEY ([ApprenticeshipLocationId], [RegionId])
)
