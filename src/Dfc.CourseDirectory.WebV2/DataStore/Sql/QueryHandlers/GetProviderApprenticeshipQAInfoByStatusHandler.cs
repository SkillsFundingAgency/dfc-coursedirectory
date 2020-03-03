using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
    s.ProviderId,
    s.ApprenticeshipQAStatus,
    s.SubmittedOn,
    u.UserId,
    u.Email,
    u.FirstName,
    u.LastName
FROM Pttcd.ApprenticeshipQASubmissions s
LEFT JOIN Pttcd.Users u ON s.LastAssessedBy = u.UserId
WHERE s.ApprenticeshipQAStatus IN @Statuses
ORDER BY s.SubmittedOn";

            var paramz = new { query.Statuses };

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
