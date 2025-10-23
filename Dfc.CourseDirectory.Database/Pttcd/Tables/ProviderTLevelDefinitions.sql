CREATE TABLE [Pttcd].[ProviderTLevelDefinitions] (
    [ProviderId]         UNIQUEIDENTIFIER NOT NULL,
    [TLevelDefinitionId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_ProviderTLevelDefinitions] PRIMARY KEY CLUSTERED ([ProviderId] ASC, [TLevelDefinitionId] ASC)
);



