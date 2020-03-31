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
CASE  assessment.Passed WHEN 1 THEN assessment.AssessedOn ELSE null end AS PassedQAOn,
CASE  assessment.Passed WHEN 0 THEN assessment.AssessedOn ELSE null end AS FailedQAOn,
UTC.AddedOn as UnableToCompleteOn, 
UTC.Comments as Notes, 
UTC.UnableToCompleteReasons, 
users.Email, 
p.ApprenticeshipQAStatus AS QAStatus
FROM [pttcd].[Providers] p
LEFT JOIN [Pttcd].[ApprenticeshipQAUnableToCompleteInfo] UTC ON UTC.ProviderID = p.ProviderID
LEFT JOIN (
SELECT ProviderId, MAX(ApprenticeshipQASubmissionId) LatestApprenticeshipQASubmissionId, SubmittedByUserId
FROM Pttcd.ApprenticeshipQASubmissions
GROUP BY ProviderId, SubmittedByUserId
) x ON  p.ProviderId = x.ProviderId
LEFT JOIN [Pttcd].[ApprenticeshipQASubmissionProviderAssessments] assessment on x.LatestApprenticeshipQASubmissionId = assessment.ApprenticeshipQASubmissionId
LEFT JOIN [Pttcd].[Users] users on users.UserId=x.SubmittedByUserId
WHERE p.ApprenticeshipQAStatus <> 1";

            return (await transaction.Connection.QueryAsync<GetQAStatusReportResult>(sql, transaction: transaction)).AsList();

        }
    }
}