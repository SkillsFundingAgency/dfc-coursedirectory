CREATE TABLE [LARS].[Validity](
	[LearnAimRef] [nvarchar](20) NOT NULL,
	[ValidityCategory] [nvarchar](50) NULL,
	[StartDate] [datetime]  NOT NULL,
	[EndDate] [datetime] NULL,
	[LastNewStartDate] [datetime] NULL,
	[Created_On] [nvarchar](50) NOT NULL,
	[Created_By] [nvarchar](50) NOT NULL,
	[Modified_By] [nvarchar](50) NULL,
	[Modified_On] [nvarchar](50) NULL
);

