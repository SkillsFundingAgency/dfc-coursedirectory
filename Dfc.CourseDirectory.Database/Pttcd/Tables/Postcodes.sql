CREATE TABLE [Pttcd].[Postcodes]
(
	[Postcode] VARCHAR(8) NOT NULL CONSTRAINT [PK_Postcodes] PRIMARY KEY,
	[Position] GEOGRAPHY NOT NULL,
	[InEngland] BIT NOT NULL
)
