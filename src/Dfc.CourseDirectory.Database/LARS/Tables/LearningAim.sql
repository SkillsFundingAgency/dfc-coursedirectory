CREATE TABLE [LARS].[LearningAim] (
    [LearningAimRefId]            VARCHAR (10)   NOT NULL,
    [Qualification]               VARCHAR (150)  NOT NULL,
    [LearningAimTitle]            NVARCHAR (255) NOT NULL,
    [LearningAimAwardOrgCode]     NVARCHAR (20)  NOT NULL,
    [ErAppStatus]                 NVARCHAR (50)  NULL,
    [SkillsForLife]               NVARCHAR (5)   NULL,
    [QualificationTypeId]         INT            NULL,
    [IndependentLivingSkills]     BIT            NOT NULL,
    [LearnDirectClassSystemCode1] NVARCHAR (12)  NULL,
    [LearnDirectClassSystemCode2] NVARCHAR (12)  NULL,
    [LearnDirectClassSystemCode3] NVARCHAR (12)  NULL,
    [RecordStatusId]              INT            NOT NULL,
    [QualificationLevelId]        INT            NULL,
    CONSTRAINT [PK_LearningAimReference] PRIMARY KEY CLUSTERED ([LearningAimRefId] ASC)
);

