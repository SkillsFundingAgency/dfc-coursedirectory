CREATE TABLE [Pttcd].[Regions]
(
	[RegionId] VARCHAR(12) NOT NULL CONSTRAINT [PK_Regions] PRIMARY KEY,
	[Name] NVARCHAR(100) NOT NULL,
	[ParentRegionId] VARCHAR(12) CONSTRAINT [FK_Regions_ParentRegion] FOREIGN KEY REFERENCES [Pttcd].[Regions] ([RegionId])
)
