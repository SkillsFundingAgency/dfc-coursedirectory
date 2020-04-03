CREATE TABLE [Tribal].[CourseMigration] (
    [Ukprn]          INT NOT NULL,
    [ReadyToMigrate] BIT NULL,
    CONSTRAINT [PK_UkprnCourseMigration] PRIMARY KEY CLUSTERED ([Ukprn] ASC)
);

