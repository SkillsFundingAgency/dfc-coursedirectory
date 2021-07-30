CREATE INDEX [IX_CourseRuns_Course] ON [Pttcd].[CourseRuns] ([CourseId]) INCLUDE ([CourseRunStatus], [StartDate])
