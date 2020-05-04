CREATE TABLE [Tribal].[ProviderUser] (
    [UserId]     NVARCHAR (450) NOT NULL,
    [ProviderId] INT            NOT NULL,
    CONSTRAINT [FK_ProviderUser_Provider] FOREIGN KEY ([ProviderId]) REFERENCES [Tribal].[Provider] ([ProviderId]),
    CONSTRAINT [FK_ProviderUser_User] FOREIGN KEY ([UserId]) REFERENCES [Identity].[AspNetUsers] ([Id])
);

