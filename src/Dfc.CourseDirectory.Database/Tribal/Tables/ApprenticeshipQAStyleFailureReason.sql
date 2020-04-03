CREATE TABLE [Tribal].[ApprenticeshipQAStyleFailureReason](
	[ApprenticeshipQAStyleId] [int] NOT NULL,
	[QAStyleFailureReason] NVARCHAR(280) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ApprenticeshipQAStyleId] ASC,
	[QAStyleFailureReason] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO