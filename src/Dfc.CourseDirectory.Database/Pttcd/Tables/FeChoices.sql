 CREATE TABLE [Pttcd].[FeChoices]
 (
    ID UniqueIdentifier NOT NULL CONSTRAINT [PK_FeChoices] PRIMARY KEY,
	Ukprn INT NOT NULL,
	LearnerSatisfaction DECIMAL(2,1) NULL,
	EmployerSatisfaction DECIMAL(2,1) NULL,
    CreatedDateTimeUtc DATETIME NOT NULL,
    CreatedOn DATETIME NOT NULL,
    CreatedBy NVARCHAR(MAX) NULL,
    LastUpdatedBy NVARCHAR(MAX) NULL,
    LastUpdatedOn DATETIME NULL
)
