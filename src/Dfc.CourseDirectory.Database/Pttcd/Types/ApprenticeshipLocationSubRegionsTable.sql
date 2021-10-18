CREATE TYPE [Pttcd].[ApprenticeshipLocationSubRegionsTable] AS TABLE
(
	[ApprenticeshipLocationId] UNIQUEIDENTIFIER NOT NULL,
	[RegionId] VARCHAR(9) NOT NULL
)
