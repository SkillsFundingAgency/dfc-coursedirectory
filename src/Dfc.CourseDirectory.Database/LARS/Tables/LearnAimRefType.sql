CREATE TABLE [LARS].[LearnAimRefType] (
    [LearnAimRefType]      NVARCHAR (50)  NOT NULL,
    [LearnAimRefTypeDesc]  NVARCHAR (100) NOT NULL,
    [LearnAimRefTypeDesc2] NVARCHAR (50)  NOT NULL,
    [EffectiveFrom]        NVARCHAR (50)  NOT NULL,
    [EffectiveTo]          NVARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_LearnAimRefType] PRIMARY KEY CLUSTERED ([LearnAimRefType] ASC)
);

