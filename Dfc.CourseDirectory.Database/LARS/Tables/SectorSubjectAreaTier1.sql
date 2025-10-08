CREATE TABLE [LARS].[SectorSubjectAreaTier1] (
    [SectorSubjectAreaTier1]      NVARCHAR (50)  NOT NULL,
    [SectorSubjectAreaTier1Desc]  NVARCHAR (100) NOT NULL,
    [SectorSubjectAreaTier1Desc2] NVARCHAR (50)  NOT NULL,
    [EffectiveFrom]               NVARCHAR (50)  NOT NULL,
    [EffectiveTo]                 NVARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_SectorSubjectAreaTier1] PRIMARY KEY CLUSTERED ([SectorSubjectAreaTier1] ASC)
);

