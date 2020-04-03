CREATE TABLE [LARS].[AwardOrgCode] (
    [AwardOrgCode]                       NVARCHAR (30)  NULL,
    [AwardOrgUKPRN]                      NVARCHAR (50)  NOT NULL,
    [AwardOrgName]                       NVARCHAR (150) NOT NULL,
    [AwardOrgShortName]                  NVARCHAR (50)  NOT NULL,
    [AwardOrgAcronym]                    NVARCHAR (50)  NOT NULL,
    [AwardOrgNonExtant]                  NVARCHAR (50)  NOT NULL,
    [AwardOrgNotes]                      NVARCHAR (100) NOT NULL,
    [AwardOrgHigherEducationInstitution] NVARCHAR (50)  NOT NULL,
    [EffectiveFrom]                      NVARCHAR (50)  NOT NULL,
    [EffectiveTo]                        NVARCHAR (50)  NOT NULL
);


GO
CREATE NONCLUSTERED INDEX [IDX_AwardOrgCode]
    ON [LARS].[AwardOrgCode]([AwardOrgCode] ASC);

