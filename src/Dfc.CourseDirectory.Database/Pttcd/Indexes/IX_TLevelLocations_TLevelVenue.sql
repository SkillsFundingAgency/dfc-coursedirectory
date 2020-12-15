CREATE UNIQUE INDEX [IX_TLevelLocations_TLevelVenue]
	ON [Pttcd].[TLevelLocations]
	([TLevelId], [VenueId])
	WHERE [TLevelLocationStatus] = 1  -- Live
