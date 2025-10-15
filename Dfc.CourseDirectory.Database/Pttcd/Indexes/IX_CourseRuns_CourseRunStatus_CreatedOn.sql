CREATE NONCLUSTERED INDEX [IX_CourseRuns_CourseRunStatus_CreatedOn] ON [Pttcd].[CourseRuns] ([CourseRunStatus], [CreatedOn]) WITH (ONLINE = ON)
