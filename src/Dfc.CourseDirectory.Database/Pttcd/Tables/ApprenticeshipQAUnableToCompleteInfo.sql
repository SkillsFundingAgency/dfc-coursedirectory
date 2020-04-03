CREATE TABLE [Pttcd].[ApprenticeshipQAUnableToCompleteInfo]
(
	[ApprenticeshipQAUnableToCompleteId] INT IDENTITY NOT NULL PRIMARY KEY,
	[ProviderId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [FK_ApprenticeshipQAUnableToCompleteInfo_Provider] FOREIGN KEY REFERENCES [Pttcd].[Providers] ([ProviderId]),
	[UnableToCompleteReasons] INT NOT NULL,
	[Comments] NVARCHAR(MAX),
	[StandardName] NVARCHAR(MAX),
	[AddedOn] DATETIME NOT NULL,
	[AddedByUserId] VARCHAR(100) NOT NULL CONSTRAINT [FK_ApprenticeshipQAUnableToCompleteInfo_AddedByUser] FOREIGN KEY REFERENCES [Pttcd].[Users] ([UserId])
)
