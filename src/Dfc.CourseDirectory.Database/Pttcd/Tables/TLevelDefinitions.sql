CREATE TABLE [Pttcd].[TLevelDefinitions] (
    [TLevelDefinitionId]         UNIQUEIDENTIFIER NOT NULL,
    [FrameworkCode]              INT              NOT NULL,
    [ProgType]                   INT              NOT NULL,
    [Name]                       NVARCHAR (MAX)   NOT NULL,
    [ExemplarWhoFor]             NVARCHAR(500),
    [ExemplarEntryRequirements]  NVARCHAR(500),
    [ExemplarWhatYoullLearn]     NVARCHAR(1500),
    [ExemplarHowYoullLearn]      NVARCHAR(500),
    [ExemplarHowYoullBeAssessed] NVARCHAR(500),
    [ExemplarWhatYouCanDoNext]   NVARCHAR(500),
    CONSTRAINT [PK_Pttcd.TLevelDefinitions] PRIMARY KEY CLUSTERED ([TLevelDefinitionId] ASC)
);

