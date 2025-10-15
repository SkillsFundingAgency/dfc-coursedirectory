CREATE TRIGGER [Pttcd].[TRG_Venues_UpdateFindACourseIndex]
ON [Pttcd].[Venues]
AFTER UPDATE
AS
BEGIN

	SET NOCOUNT ON

	-- Empty trigger as {pre}prod deployment won't remove objects

END
