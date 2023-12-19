CREATE TABLE [Pttcd].[NonLarsSubType]
(
	[NonLarsSubTypeId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [Name] NCHAR(10) NOT NULL, 
    [AddedOn] DATETIME NOT NULL, 
    [UpdatedOn] DATETIME NULL, 
    [IsActive] tinyint NOT NULL DEFAULT(0),
)
