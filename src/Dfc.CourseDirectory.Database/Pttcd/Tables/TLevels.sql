CREATE TABLE [Pttcd].[TLevels]
(
	[TLevelId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_TLevels] PRIMARY KEY,
	[TLevelStatus] TINYINT NOT NULL,
	[CreatedOn] DATETIME NOT NULL,
	[CreatedByUserId] VARCHAR(100) NOT NULL CONSTRAINT [FK_TLevels_CreatedBy] FOREIGN KEY REFERENCES [Pttcd].[Users] (UserId),
	[UpdatedOn] DATETIME NOT NULL,
	[UpdatedByUserId] VARCHAR(100) NOT NULL CONSTRAINT [FK_TLevels_UpdatedBy] FOREIGN KEY REFERENCES [Pttcd].[Users] (UserId),
	[DeletedOn] DATETIME,
	[DeletedByUserId] VARCHAR(100) CONSTRAINT [FK_TLevels_DeletdBy] FOREIGN KEY REFERENCES [Pttcd].[Users] (UserId),
	[ProviderId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [FK_TLevels_Provider] FOREIGN KEY REFERENCES [Pttcd].[Providers] ([ProviderId]),
	[TLevelDefinitionId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [FK_TLevels_TLevelDefinition] FOREIGN KEY  REFERENCES [Pttcd].[TLevelDefinitions] ([TLevelDefinitionId]),
	[WhoFor] NVARCHAR(500),
	[EntryRequirements] NVARCHAR(500),
	[WhatYoullLearn] NVARCHAR(1500),
	[HowYoullLearn] NVARCHAR(500),
	[HowYoullBeAssessed] NVARCHAR(500),
	[WhatYouCanDoNext] NVARCHAR(500),
	[YourReference] NVARCHAR(255),
	[StartDate] DATE NOT NULL,
	[Website] NVARCHAR(255)
)
