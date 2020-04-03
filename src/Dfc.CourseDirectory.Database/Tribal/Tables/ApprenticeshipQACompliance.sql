CREATE TABLE [Tribal].[ApprenticeshipQACompliance](
	[ApprenticeshipQAComplianceId] [int] NOT NULL,
	[ApprenticeshipId] [int] NOT NULL,
	[CreatedByUserEmail] NVARCHAR(280) NOT NULL,
	[CreatedDateTimeUtc] [datetime] NOT NULL,
	[TextQAd] [nvarchar](900) NULL,
	[DetailsOfUnverifiableClaim] [nvarchar](2000) NULL,
	[DetailsOfComplianceFailure] [nvarchar](2000) NULL,
	[Passed] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ApprenticeshipQAComplianceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO