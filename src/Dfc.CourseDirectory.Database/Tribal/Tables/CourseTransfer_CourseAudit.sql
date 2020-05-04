CREATE TABLE [Tribal].[CourseTransfer_CourseAudit] (
    [CourseMigrationCourseAuditId] INT            IDENTITY (1, 1) NOT NULL,
    [CourseTransferId]             INT            NULL,
    [Ukprn]                        INT            NULL,
    [CourseId]                     INT            NULL,
    [LARS]                         VARCHAR (10)   NULL,
    [CourseRecordStatus]           INT            NULL,
    [CourseRuns]                   INT            NULL,
    [CourseRunsLive]               INT            NULL,
    [CourseRunsPending]            INT            NULL,
    [CourseRunsReadyToGoLive]      INT            NULL,
    [MigrationSuccess]             INT            NULL,
    [CourseMigrationNote]          NVARCHAR (MAX) NULL,
    [CourseRunsLARSless]           INT            NULL,
    CONSTRAINT [PK_CourseTransfer_CourseAudit] PRIMARY KEY CLUSTERED ([CourseMigrationCourseAuditId] ASC)
);

