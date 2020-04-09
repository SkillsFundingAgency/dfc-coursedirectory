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
CASE  assessment.Passed WHEN 1 THEN provassess.AssessedOn ELSE null end AS PassedQAOn,
CASE  assessment.Passed WHEN 0 THEN provassess.AssessedOn ELSE null end AS FailedQAOn,
reasons.AddedOn as UnableToCompleteOn, 
reasons.Comments as Notes, 
reasons.UnableToCompleteReasons, 
users.Email, 
p.ApprenticeshipQAStatus AS QAStatus
FROM [pttcd].[Providers] p
LEFT JOIN (
    SELECT ProviderId, MAX(ApprenticeshipQASubmissionId) ApprenticeshipQASubmissionId, SubmittedByUserId
    FROM Pttcd.ApprenticeshipQASubmissions
    GROUP BY ProviderId,SubmittedByUserId
) LatestSubmissions ON p.ProviderId = LatestSubmissions.ProviderId
LEFT JOIN (
    SELECT ProviderId, MAX(ApprenticeshipQAUnableToCompleteId) ApprenticeshipQAUnableToCompleteId
    FROM Pttcd.ApprenticeshipQAUnableToCompleteInfo
    GROUP BY ProviderId
) LatestUnableToComplete ON p.ProviderId = LatestUnableToComplete.ProviderId
LEFT JOIN Pttcd.ApprenticeshipQAUnableToCompleteInfo reasons on reasons.ApprenticeshipQAUnableToCompleteId = LatestUnableToComplete.ApprenticeshipQAUnableToCompleteId
LEFT JOIN (
    SELECT ApprenticeshipQASubmissionId, MAX(ApprenticeshipQASubmissionProviderAssessmentsId) ApprenticeshipQASubmissionProviderAssessmentsId
    FROM Pttcd.ApprenticeshipQASubmissionProviderAssessments
    GROUP BY ApprenticeshipQASubmissionId
) LatestAssessment ON LatestAssessment.ApprenticeshipQASubmissionId = LatestSubmissions.ApprenticeshipQASubmissionId
LEFT JOIN [Pttcd].ApprenticeshipQASubmissions assessment on assessment.ApprenticeshipQASubmissionId = LatestAssessment.ApprenticeshipQASubmissionId
LEFT JOIN [Pttcd].ApprenticeshipQASubmissionProviderAssessments provassess on provassess.ApprenticeshipQASubmissionProviderAssessmentsId=LatestAssessment.ApprenticeshipQASubmissionProviderAssessmentsId
LEFT JOIN [Pttcd].[Users] users on users.UserId=LatestSubmissions.SubmittedByUserId
WHERE p.ApprenticeshipQAStatus <> 1";

            return (await transaction.Connection.QueryAsync<GetQAStatusReportResult>(sql, transaction: transaction)).AsList();

        }
    }
}