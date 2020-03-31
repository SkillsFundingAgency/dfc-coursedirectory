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
    public class GetProviderApprenticeshipQAInfoByStatusHandler
        : ISqlQueryHandler<GetProviderApprenticeshipQAInfoByStatus, IReadOnlyCollection<GetProviderApprenticeshipQAInfoByStatusResult>>
    {
        public async Task<IReadOnlyCollection<GetProviderApprenticeshipQAInfoByStatusResult>> Execute(
            SqlTransaction transaction,
            GetProviderApprenticeshipQAInfoByStatus query)
        {
            var sql = @"
SELECT
    p.ProviderId,
    p.ApprenticeshipQAStatus,
    s.SubmittedOn,
    u.UserId,
    u.Email,
    u.FirstName,
    u.LastName
FROM Pttcd.Providers p
LEFT JOIN Pttcd.ApprenticeshipQASubmissions s ON p.ProviderId = s.ProviderId
LEFT JOIN (
    SELECT ProviderId, MAX(ApprenticeshipQASubmissionId) LatestApprenticeshipQASubmissionId
    FROM Pttcd.ApprenticeshipQASubmissions
    GROUP BY ProviderId
) x ON s.ApprenticeshipQASubmissionId = x.LatestApprenticeshipQASubmissionId AND s.ProviderId = x.ProviderId
LEFT JOIN Pttcd.Users u ON s.LastAssessedByUserId = u.UserId
WHERE p.ApprenticeshipQAStatus & @StatusMask != 0
ORDER BY s.SubmittedOn DESC";

            var statusMask = query.Statuses.Aggregate(
                (ApprenticeshipQAStatus)0,
                (s, combined) => combined | s);

            var paramz = new { StatusMask = statusMask };

            return (await transaction.Connection.QueryAsync<QASubmissionResult, UserInfo, GetProviderApprenticeshipQAInfoByStatusResult>(
                sql,
                (r, u) => new GetProviderApprenticeshipQAInfoByStatusResult()
                {
                    ApprenticeshipQAStatus = r.ApprenticeshipQAStatus,
                    LastAssessedBy = u,
                    ProviderId = r.ProviderId,
                    SubmittedOn = r.SubmittedOn
                },
                paramz,
                transaction,
                splitOn: "UserId")).AsList();
        }

        private class QASubmissionResult
        {
            public Guid ProviderId { get; set; }
            public ApprenticeshipQAStatus ApprenticeshipQAStatus { get; set; }
            public DateTime? SubmittedOn { get; set; }
        }
    }
}
