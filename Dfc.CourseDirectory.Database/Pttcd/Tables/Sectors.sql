CREATE TABLE [Pttcd].[Sectors]
(
	[Id] TINYINT NOT NULL CONSTRAINT [PK_Sectors] PRIMARY KEY, 
    [Code] NVARCHAR(50) NULL, 
    [Description] NVARCHAR(255) NULL
)
