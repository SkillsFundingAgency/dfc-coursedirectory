CREATE TABLE [LARS].[SectorSubjectAreaTier2] (
    [SectorSubjectAreaTier2]      NVARCHAR (50)  NOT NULL,
    [SectorSubjectAreaTier2Desc]  NVARCHAR (100) NOT NULL,
    [SectorSubjectAreaTier2Desc2] NVARCHAR (50)  NOT NULL,
    [EffectiveFrom]               NVARCHAR (50)  NOT NULL,
    [EffectiveTo]                 NVARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_SectorSubjectAreaTier2] PRIMARY KEY CLUSTERED ([SectorSubjectAreaTier2] ASC)
);

