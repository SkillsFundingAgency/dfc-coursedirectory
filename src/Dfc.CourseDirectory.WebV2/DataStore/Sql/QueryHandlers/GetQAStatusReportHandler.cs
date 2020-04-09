using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.QueryHandlers
{
    public class GetQAStatusReportHandler : ISqlQueryHandler<GetQAStatusReport, IReadOnlyCollection<GetQAStatusReportResult>>
    {
        public async Task<IReadOnlyCollection<GetQAStatusReportResult>> Execute(SqlTransaction transaction, GetQAStatusReport query)
        {
            var sql = @"
SELECT p.ProviderID,
CASE  submission.Passed WHEN 1 THEN submission.LastAssessedOn ELSE null end AS PassedQAOn,
CASE  submission.Passed WHEN 0 THEN submission.LastAssessedOn ELSE null end AS FailedQAOn,
reasons.AddedOn as UnableToCompleteOn,
reasons.Comments as Notes,
CASE (p.ApprenticeshipQAStatus & 32) WHEN 0 THEN NULL ELSE reasons.UnableToCompleteReasons END AS UnableToCompleteReasons,
users.Email,
p.ApprenticeshipQAStatus AS QAStatus
FROM [pttcd].[Providers] p
LEFT JOIN (
    SELECT ProviderId, MAX(ApprenticeshipQASubmissionId) ApprenticeshipQASubmissionId
    FROM Pttcd.ApprenticeshipQASubmissions
    GROUP BY ProviderId
) LatestSubmissions ON p.ProviderId = LatestSubmissions.ProviderId
LEFT JOIN (
    SELECT ProviderId, MAX(ApprenticeshipQAUnableToCompleteId) ApprenticeshipQAUnableToCompleteId
    FROM Pttcd.ApprenticeshipQAUnableToCompleteInfo
    GROUP BY ProviderId
) LatestUnableToComplete ON p.ProviderId = LatestUnableToComplete.ProviderId
LEFT JOIN Pttcd.ApprenticeshipQAUnableToCompleteInfo reasons on reasons.ApprenticeshipQAUnableToCompleteId = LatestUnableToComplete.ApprenticeshipQAUnableToCompleteId
LEFT JOIN [Pttcd].ApprenticeshipQASubmissions submission on submission.ApprenticeshipQASubmissionId = LatestSubmissions.ApprenticeshipQASubmissionId
LEFT JOIN [Pttcd].[Users] users on users.UserId=submission.SubmittedByUserId
WHERE p.ApprenticeshipQAStatus <> 1";

            return (await transaction.Connection.QueryAsync<GetQAStatusReportResult>(sql, transaction: transaction)).AsList();

        }
    }
}