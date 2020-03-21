using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.QueryHandlers
{
    public class GetQAStatusReportHandler : ISqlQueryHandler<GetQAStatusReport, IReadOnlyCollection<GetQAStatusReportResult>>
    {
        public async Task<IReadOnlyCollection<GetQAStatusReportResult>> Execute(SqlTransaction transaction, GetQAStatusReport query)
        {
            var sql = @"
SELECT p.ProviderID, UTC.AddedOn as FailedOn, UTC.Comments, UTC.UnableToCompleteReasons, RequestedAccess.SubmittedOn as RequestedAccessOn, users.Email, p.ApprenticeshipQAStatus AS QAStatus, Assessment.AssessedOn
FROM [pttcd].[Providers] p
LEFT JOIN [Pttcd].[ApprenticeshipQAUnableToCompleteInfo] UTC ON UTC.ProviderID = p.ProviderID
OUTER APPLY 
( 
SELECT TOP 1 aqs.SubmittedOn,SubmittedByUserId
FROM [Pttcd].[ApprenticeshipQASubmissions] aqs
WHERE aqs.ProviderId = p.ProviderID
ORDER BY aqs.SubmittedOn DESC
) RequestedAccess
OUTER APPLY ( 
SELECT TOP 1 aqasub.SubmittedOn,
aqasub.SubmittedByUserId,
appassessments.AssessedOn
FROM [Pttcd].[ApprenticeshipQASubmissions] aqasub
LEFT JOIN [Pttcd].[ApprenticeshipQASubmissionProviderAssessments] appassessments on appassessments.ApprenticeshipQASubmissionId = aqasub.ApprenticeshipQASubmissionId
WHERE aqasub.ProviderId = p.ProviderID
ORDER BY aqasub.SubmittedOn DESC
) Assessment
LEFT JOIN [Pttcd].[Users] users on users.UserId=RequestedAccess.SubmittedByUserId";

            return (await transaction.Connection.QueryAsync<GetQAStatusReportResult>(sql, transaction)).AsList();
        }
    }
}
