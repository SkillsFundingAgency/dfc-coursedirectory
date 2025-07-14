CREATE NONCLUSTERED INDEX [IX_FindACourseIndex_Live_LastSynced] ON [Pttcd].[FindACourseIndex] ([Live], [LastSynced]) WITH (ONLINE = ON)
