CREATE UNIQUE INDEX [IX_TLevels_DefinitionProviderStartDate]
	ON [Pttcd].[TLevels]
	([ProviderId], [TLevelDefinitionId], [StartDate])
	WHERE [TLevelStatus] = 1  -- Live
