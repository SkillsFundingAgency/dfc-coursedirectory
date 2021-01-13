CREATE TRIGGER [Pttcd].[TRG_TLevels_UpdateFindACourseIndex]
ON [Pttcd].[TLevels]
AFTER INSERT, UPDATE
AS
BEGIN

	DECLARE @TLevelIds Pttcd.GuidIdTable,
			@Now DATETIME

	INSERT INTO @TLevelIds
	SELECT TLevelId FROM inserted

	SET @Now = GETUTCDATE()

	EXEC Pttcd.RefreshFindACourseIndexForTLevels @TLevelIds, @Now

END
