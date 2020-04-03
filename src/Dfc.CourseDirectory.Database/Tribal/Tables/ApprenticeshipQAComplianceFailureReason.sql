CREATE TABLE [Tribal].[ApprenticeshipQAComplianceFailureReason](
	[ApprenticeshipQAComplianceId] [int] NOT NULL,
	[QAComplianceFailureReason] NVARCHAR(100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ApprenticeshipQAComplianceId] ASC,
	[QAComplianceFailureReason] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]