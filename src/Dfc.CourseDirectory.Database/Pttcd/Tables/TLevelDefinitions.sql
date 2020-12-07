CREATE TABLE [Pttcd].[TLevelDefinitions] (
    [TLevelDefinitionId] UNIQUEIDENTIFIER NOT NULL,
    [FrameworkCode]      INT              NOT NULL,
    [ProgType]           INT              NOT NULL,
    [Name]               NVARCHAR (MAX)   NOT NULL,
    CONSTRAINT [PK_Pttcd.TLevelDefinitions] PRIMARY KEY CLUSTERED ([TLevelDefinitionId] ASC)
);

