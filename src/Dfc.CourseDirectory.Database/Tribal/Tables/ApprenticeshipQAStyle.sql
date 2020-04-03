CREATE TABLE [Tribal].[ApprenticeshipQAStyle](
	[ApprenticeshipQAStyleId] [int] NOT NULL,
	[ApprenticeshipId] [int] NOT NULL,
	[CreatedByUserEmail] [nvarchar](280) NOT NULL,
	[CreatedDateTimeUtc] [datetime] NOT NULL,
	[TextQAd] [nvarchar](900) NULL,
	[Passed] [bit] NOT NULL,
	[DetailsOfQA] [nvarchar](1000) NULL,
PRIMARY KEY CLUSTERED 
(
	[ApprenticeshipQAStyleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO