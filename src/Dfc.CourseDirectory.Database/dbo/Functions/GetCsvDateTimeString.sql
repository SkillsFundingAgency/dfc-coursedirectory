

CREATE FUNCTION [dbo].[GetCsvDateTimeString](
	@DateTime DATETIME
	)
RETURNS VARCHAR(20)
AS 
BEGIN
	-- Used to create the date time string used in the CSV outputs
	DECLARE @Output VARCHAR(20)
	IF(@DateTime IS NULL)
	BEGIN
		SET @Output = ''
	END
	ELSE
	BEGIN
		SET @Output = CONVERT(VARCHAR(20), @DateTime, 120)
		-- Remove the dashes
		SET @Output = REPLACE(@Output, '-', '')
	END
	RETURN @Output	
END