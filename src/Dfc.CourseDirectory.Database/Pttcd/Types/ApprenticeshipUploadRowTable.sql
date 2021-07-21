CREATE TYPE [Pttcd].[ApprenticeshipUploadRowTable] AS TABLE
(
	[RowNumber] INT NOT NULL,
	[IsValid] BIT NOT NULL,
	[Errors] VARCHAR(MAX),
	[LastUpdated] DATETIME,
	[LastValidated] DATETIME NOT NULL,
    [StandardCode] INT NOT NULL,
    [StandardVersion] INT NOT NULL,
    [ApprenticeshipInformation] NVARCHAR(MAX) NOT NULL,
    [ApprenticeshipWebpage]  NVARCHAR(MAX) NULL,
    [ContactEmail] NVARCHAR(MAX) NOT NULL,
    [ContactPhone] NVARCHAR(MAX) NOT NULL,
    [ContactUrl] NVARCHAR(MAX) NULL,
    [DeliveryMode] NVARCHAR(MAX) NOT NULL,
    [Venue] NVARCHAR(MAX) NULL,
    [YourVenueReference] NVARCHAR(MAX) NOT NULL,
    [Radius] NVARCHAR(MAX) NULL,
    [NationalDelivery] NVARCHAR(MAX) NULL,
    [SubRegion] NVARCHAR(MAX) NULL
)
