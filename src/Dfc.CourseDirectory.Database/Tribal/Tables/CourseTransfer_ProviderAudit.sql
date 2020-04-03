CREATE TABLE [Tribal].[CourseTransfer_ProviderAudit] (
    [CourseMigrationProviderAuditId] INT            IDENTITY (1, 1) NOT NULL,
    [CourseTransferId]               INT            NULL,
    [Ukprn]                          INT            NULL,
    [CoursesToBeMigrated]            INT            NULL,
    [CoursesGoodToBeMigrated]        INT            NULL,
    [CoursesGoodToBeMigratedPending] INT            NULL,
    [CoursesGoodToBeMigratedLive]    INT            NULL,
    [CoursesNotGoodToBeMigrated]     INT            NULL,
    [MigrationSuccesses]             INT            NULL,
    [MigrationFailures]              INT            NULL,
    [TimeTaken]                      VARCHAR (50)   NULL,
    [ProviderReportFileName]         VARCHAR (255)  NULL,
    [MigrationNote]                  NVARCHAR (MAX) NULL,
    [CoursesLARSless]                INT            NULL,
    CONSTRAINT [PK_UkprnCourseMigration_Audit] PRIMARY KEY CLUSTERED ([CourseMigrationProviderAuditId] ASC),
    CONSTRAINT [FK_CourseTransfer_ProviderAudit_CourseTransfer] FOREIGN KEY ([CourseTransferId]) REFERENCES [Tribal].[CourseTransfer] ([CourseTransferId])
);

