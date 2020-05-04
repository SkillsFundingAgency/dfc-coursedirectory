CREATE TABLE [LARS].[Category] (
    [CategoryRef]       NVARCHAR (50)  NOT NULL,
    [ParentCategoryRef] NVARCHAR (50)  NOT NULL,
    [CategoryName]      NVARCHAR (100) NOT NULL,
    [Target]            NVARCHAR (50)  NOT NULL,
    [EffectiveFrom]     NVARCHAR (50)  NOT NULL,
    [EffectiveTo]       NVARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED ([CategoryRef] ASC)
);

