CREATE TABLE [dbo].[Venue] (
    [VenueId]             INT             NOT NULL,
    [ProviderId]          INT             NOT NULL,
    [ProviderOwnVenueRef] NVARCHAR (255)  NULL,
    [VenueName]           NVARCHAR (255)  NOT NULL,
    [Email]               NVARCHAR (255)  NULL,
    [Website]             NVARCHAR (255)  NULL,
    [Fax]                 NVARCHAR (35)   NULL,
    [Facilities]          NVARCHAR (2000) NULL,
    [RecordStatusId]      INT             NOT NULL,
    [CreatedByUserId]     NVARCHAR (128)  NOT NULL,
    [CreatedDateTimeUtc]  DATETIME        NOT NULL,
    [ModifiedByUserId]    NVARCHAR (128)  NULL,
    [ModifiedDateTimeUtc] DATETIME        NULL,
    [AddressId]           INT             NOT NULL,
    [Telephone]           NVARCHAR (30)   NULL,
    [BulkUploadVenueId]   NVARCHAR (255)  NULL,
    CONSTRAINT [PK_Venue] PRIMARY KEY CLUSTERED ([VenueId] ASC)
);

