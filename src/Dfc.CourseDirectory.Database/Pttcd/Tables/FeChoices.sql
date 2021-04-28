 CREATE TABLE [Pttcd].[FeChoices]
 (
    ID UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_FeChoices] PRIMARY KEY,
    Ukprn INT NOT NULL,
    LearnerSatisfaction DECIMAL(18,2) NULL,
    EmployerSatisfaction DECIMAL(18,2) NULL,
    CreatedDateTimeUtc DATETIME NOT NULL
)
