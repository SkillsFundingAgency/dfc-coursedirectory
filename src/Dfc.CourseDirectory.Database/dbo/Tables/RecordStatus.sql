CREATE TABLE [dbo].[RecordStatus] (
    [RecordStatusId]   INT          NOT NULL,
    [RecordStatusName] VARCHAR (50) NULL,
    [IsPublished]      BIT          NOT NULL,
    [IsArchived]       BIT          NOT NULL,
    [IsDeleted]        BIT          NOT NULL,
    CONSTRAINT [PK_RecordStatus] PRIMARY KEY CLUSTERED ([RecordStatusId] ASC)
);

