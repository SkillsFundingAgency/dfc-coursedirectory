CREATE TABLE [Pttcd].[StandardSectorCodes] (
    [StandardSectorCode]      INT            NOT NULL,
    [StandardSectorCodeDesc]  NVARCHAR (MAX) NOT NULL,
    [StandardSectorCodeDesc2] NVARCHAR (MAX) NOT NULL,
    [EffectiveFrom]           DATE           NOT NULL,
    [EffectiveTo]             DATE           NULL,
    CONSTRAINT [PK_StandardSectorCodes] PRIMARY KEY CLUSTERED ([StandardSectorCode] ASC)
);

