CREATE TABLE [dbo].[Address] (
    [AddressId]        INT            NOT NULL,
    [AddressLine1]     NVARCHAR (110) NULL,
    [AddressLine2]     NVARCHAR (100) NULL,
    [Town]             NVARCHAR (75)  NULL,
    [County]           NVARCHAR (75)  NULL,
    [Postcode]         NVARCHAR (30)  NOT NULL,
    [ProviderRegionId] INT            NULL,
    [Latitude]         FLOAT (53)     NULL,
    [Longitude]        FLOAT (53)     NULL,
    [Geography]        AS             ([geography]::STGeomFromText(((('Point('+CONVERT([varchar](32),[Longitude]))+' ')+CONVERT([varchar](32),[Latitude]))+')',(4326))),
    CONSTRAINT [PK_Address_computed] PRIMARY KEY CLUSTERED ([AddressId] ASC),
    CONSTRAINT [FK_Address_ProviderRegion] FOREIGN KEY ([ProviderRegionId]) REFERENCES [dbo].[ProviderRegion] ([ProviderRegionId])
);

