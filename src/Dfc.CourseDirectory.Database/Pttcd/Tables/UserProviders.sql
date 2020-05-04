CREATE TABLE [Pttcd].[UserProviders]
(
	[UserId] VARCHAR(100) NOT NULL CONSTRAINT [FK_UserProviders_User] FOREIGN KEY REFERENCES [Pttcd].[Users] ([UserId]),
	[ProviderId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [FK_UserProviders_Provider] FOREIGN KEY REFERENCES [Pttcd].[Providers] ([ProviderId])
)
