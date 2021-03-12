CREATE TABLE [Pttcd].[Standards] (
    [StandardCode]              INT            NOT NULL,
    [Version]                   INT            NOT NULL,
    [StandardName]              NVARCHAR (MAX) NOT NULL,
    [StandardSectorCode]        NVARCHAR (MAX) NOT NULL,
    [NotionalEndLevel]          NVARCHAR (MAX) NOT NULL,
    [EffectiveFrom]             DATE           NOT NULL,
    [EffectiveTo]               DATE           NULL,
    [UrlLink]                   NVARCHAR (MAX) NOT NULL,
    [SectorSubjectAreaTier1]    DECIMAL (9, 2) NOT NULL,
    [SectorSubjectAreaTier2]    DECIMAL (9, 2) NOT NULL,
    [OtherBodyApprovalRequired] BIT            NULL,
    CONSTRAINT [PK_Standards] PRIMARY KEY CLUSTERED ([StandardCode] ASC, [Version] ASC)
);

