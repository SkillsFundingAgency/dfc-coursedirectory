using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
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
    FirstSignIn.SignedInUtc AddedOn,
    u.UserId,
    u.Email,
    u.FirstName,
    u.LastName,
    utci.UnableToCompleteReasons
FROM Pttcd.Providers p
JOIN (
    SELECT MIN(usi.SignedInUtc) SignedInUtc, up.ProviderId
    FROM Pttcd.UserSignIns usi
    JOIN Pttcd.UserProviders up ON usi.UserId = up.UserId
    GROUP BY up.ProviderId
) FirstSignIn ON p.ProviderId = FirstSignIn.ProviderId
LEFT JOIN (
    SELECT ProviderId, MAX(ApprenticeshipQASubmissionId) ApprenticeshipQASubmissionId
    FROM Pttcd.ApprenticeshipQASubmissions
    GROUP BY ProviderId
) LatestSubmissions ON p.ProviderId = LatestSubmissions.ProviderId
LEFT JOIN Pttcd.ApprenticeshipQASubmissions s ON p.ProviderId = s.ProviderId
AND s.ApprenticeshipQASubmissionId = LatestSubmissions.ApprenticeshipQASubmissionId
LEFT JOIN (
    SELECT ProviderId, MAX(ApprenticeshipQAUnableToCompleteId) ApprenticeshipQAUnableToCompleteId
    FROM Pttcd.ApprenticeshipQAUnableToCompleteInfo
    GROUP BY ProviderId
) LatestUnableToComplete ON p.ProviderId = LatestUnableToComplete.ProviderId
LEFT JOIN Pttcd.ApprenticeshipQAUnableToCompleteInfo utci ON LatestUnableToComplete.ApprenticeshipQAUnableToCompleteId = utci.ApprenticeshipQAUnableToCompleteId
LEFT JOIN Pttcd.Users u ON s.LastAssessedByUserId = u.UserId
WHERE p.ApprenticeshipQAStatus & @StatusMask != 0";

            var statusMask = query.Statuses.Aggregate(
                (ApprenticeshipQAStatus)0,
                (s, combined) => combined | s);

            var paramz = new { StatusMask = statusMask };

            return (await transaction.Connection.QueryAsync<QASubmissionResult, UserInfo, ApprenticeshipQAUnableToCompleteReasons?, GetProviderApprenticeshipQAInfoByStatusResult>(
                sql,
                (r, u, utcr) => new GetProviderApprenticeshipQAInfoByStatusResult()
                {
                    ApprenticeshipQAStatus = r.ApprenticeshipQAStatus,
                    LastAssessedBy = u,
                    ProviderId = r.ProviderId,
                    SubmittedOn = r.SubmittedOn,
                    AddedOn = r.AddedOn,
                    UnableToCompleteReasons = utcr
                },
                paramz,
                transaction,
                splitOn: "UserId,UnableToCompleteReasons")).AsList();
        }

        private class QASubmissionResult
        {
            public Guid ProviderId { get; set; }
            public ApprenticeshipQAStatus ApprenticeshipQAStatus { get; set; }
            public DateTime? SubmittedOn { get; set; }
            public DateTime AddedOn { get; set; }
            public ApprenticeshipQAUnableToCompleteReasons? UnableToCompleteReasons { get; set; }
        }
    }
}
