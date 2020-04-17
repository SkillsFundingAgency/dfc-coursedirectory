using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLatestApprenticeshipQASubmissionForProviderHandler
        : ISqlQueryHandler<GetLatestApprenticeshipQASubmissionForProvider, OneOf<None, ApprenticeshipQASubmission>>
    {
        public async Task<OneOf<None, ApprenticeshipQASubmission>> Execute(
            SqlTransaction transaction,
            GetLatestApprenticeshipQASubmissionForProvider query)
        {
            var sql = @"
DECLARE @LatestApprenticeshipQASubmissionId INT

SELECT TOP 1 @LatestApprenticeshipQASubmissionId = ApprenticeshipQASubmissionId
FROM Pttcd.ApprenticeshipQASubmissions s
WHERE s.ProviderId = @ProviderId
ORDER BY s.SubmittedOn DESC

SELECT TOP 1
    s.ApprenticeshipQASubmissionId,
    s.ProviderId,
    s.SubmittedOn,
    s.ProviderMarketingInformation,
    s.Passed,
    s.ProviderAssessmentPassed,
    s.ApprenticeshipAssessmentsPassed,
    s.LastAssessedOn,
    s.HidePassedNotification,
    b.UserId,
    b.Email,
    b.FirstName,
    b.LastName,
    a.UserId,
    a.Email,
    a.FirstName,
    a.LastName
FROM Pttcd.ApprenticeshipQASubmissions s
JOIN Pttcd.Users b ON s.SubmittedByUserId = b.UserId
LEFT JOIN Pttcd.Users a ON s.LastAssessedByUserId = a.UserId
WHERE s.ApprenticeshipQASubmissionId = @LatestApprenticeshipQASubmissionId

SELECT
    s.ApprenticeshipQASubmissionApprenticeshipId,
    s.ApprenticeshipId,
    s.ApprenticeshipTitle,
    s.ApprenticeshipMarketingInformation
FROM Pttcd.ApprenticeshipQASubmissionApprenticeships s
WHERE s.ApprenticeshipQASubmissionId = @LatestApprenticeshipQASubmissionId";

            var paramz = new { query.ProviderId };

            using (var reader = await transaction.Connection.QueryMultipleAsync(sql, paramz, transaction))
            {
                var submission = reader.Read<Header, UserInfo, UserInfo, ApprenticeshipQASubmission>(
                    (h, submittedBy, assessedBy) => new ApprenticeshipQASubmission()
                    {
                        ApprenticeshipAssessmentsPassed = h.ApprenticeshipAssessmentsPassed,
                        ApprenticeshipQASubmissionId = h.ApprenticeshipQASubmissionId,
                        LastAssessedBy = assessedBy,
                        LastAssessedOn = h.LastAssessedOn,
                        Passed = h.Passed,
                        ProviderAssessmentPassed = h.ProviderAssessmentPassed,
                        ProviderId = h.ProviderId,
                        ProviderMarketingInformation = h.ProviderMarketingInformation,
                        SubmittedByUser = submittedBy,
                        SubmittedOn = h.SubmittedOn,
                        HidePassedNotification = h.HidePassedNotification
                    },
                    splitOn: "UserId,UserId").SingleOrDefault();

                if (submission == null)
                {
                    return new None();
                }

                var apps = reader.Read<ApprenticeshipQASubmissionApprenticeship>();
                submission.Apprenticeships = apps.AsList();

                return submission;
            }
        }

        private class Header
        {
            public int ApprenticeshipQASubmissionId { get; set; }
            public Guid ProviderId { get; set; }
            public DateTime SubmittedOn { get; set; }
            public string ProviderMarketingInformation { get; set; }
            public bool? Passed { get; set; }
            public DateTime? LastAssessedOn { get; set; }
            public bool? ProviderAssessmentPassed { get; set; }
            public bool? ApprenticeshipAssessmentsPassed { get; set; }
            public bool HidePassedNotification { get; set; }
        }
    }
}
