CREATE TABLE [Pttcd].[NonLarsSubType]
(
	[NonLarsSubTypeId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [Name] NCHAR(50) NOT NULL, 
    [AddedOn] DATETIME NOT NULL, 
    [UpdatedOn] DATETIME NULL, 
    [IsActive] tinyint NOT NULL DEFAULT(0),
)
