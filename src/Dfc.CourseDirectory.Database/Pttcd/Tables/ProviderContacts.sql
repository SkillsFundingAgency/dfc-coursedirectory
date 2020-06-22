CREATE TABLE [Pttcd].[ProviderContacts]
(
	[ProviderContactId] BIGINT IDENTITY NOT NULL CONSTRAINT PK_ProviderContacts PRIMARY KEY,
	[ProviderId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_ProviderContacts_Provider FOREIGN KEY REFERENCES [Pttcd].[Providers] (ProviderId),
	[ProviderContactIndex] INT NOT NULL,
	[ContactType] CHAR,
	[ContactRole] NVARCHAR(MAX),
	[AddressSaonDescription] NVARCHAR(MAX),
	[AddressPaonDescription] NVARCHAR(MAX),
	[AddressStreetDescription] NVARCHAR(MAX),
	[AddressLocality] NVARCHAR(MAX),
	[AddressItems] NVARCHAR(MAX),
	[AddressPostTown] NVARCHAR(MAX),
	[AddressPostcode] NVARCHAR(MAX),
	[PersonalDetailsPersonNameTitle] NVARCHAR(MAX),
	[PersonalDetailsPersonNameGivenName] NVARCHAR(MAX),
	[PersonalDetailsPersonNameFamilyName] NVARCHAR(MAX),
)