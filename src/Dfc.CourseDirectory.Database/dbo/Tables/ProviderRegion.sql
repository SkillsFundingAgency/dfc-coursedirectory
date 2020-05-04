CREATE TABLE [dbo].[ProviderRegion] (
    [ProviderRegionId] INT            NOT NULL,
    [RegionName]       NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_ProviderRegion] PRIMARY KEY CLUSTERED ([ProviderRegionId] ASC)
);

