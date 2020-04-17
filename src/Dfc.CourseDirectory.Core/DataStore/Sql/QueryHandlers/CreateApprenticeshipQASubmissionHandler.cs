using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class CreateApprenticeshipQASubmissionHandler : ISqlQueryHandler<CreateApprenticeshipQASubmission, int>
    {
        public async Task<int> Execute(SqlTransaction transaction, CreateApprenticeshipQASubmission query)
        {
            var apprenticeshipQASubmissionId = await CreateSubmission();

            foreach (var app in query.Apprenticeships)
            {
                await CreateSubmissionApprenticeship(app);
            }

            return apprenticeshipQASubmissionId;

            Task<int> CreateSubmission()
            {
                var sql = @"
INSERT INTO Pttcd.ApprenticeshipQASubmissions
(ProviderId, SubmittedOn, SubmittedByUserId, ProviderMarketingInformation)
VALUES (@ProviderId, @SubmittedOn, @SubmittedByUserId, @ProviderMarketingInformation)

SELECT SCOPE_IDENTITY() ApprenticeshipQASubmissionId";

                var paramz = new
                {
                    query.ProviderId,
                    query.SubmittedOn,
                    query.SubmittedByUserId,
                    query.ProviderMarketingInformation
                };

                return transaction.Connection.QuerySingleAsync<int>(sql, paramz, transaction);
            }

            Task CreateSubmissionApprenticeship(CreateApprenticeshipQASubmissionApprenticeship app)
            {
                var sql = @"
INSERT INTO Pttcd.ApprenticeshipQASubmissionApprenticeships
(ApprenticeshipQASubmissionId, ApprenticeshipId, ApprenticeshipTitle, ApprenticeshipMarketingInformation)
VALUES (@ApprenticeshipQASubmissionId, @ApprenticeshipId, @ApprenticeshipTitle, @ApprenticeshipMarketingInformation)";

                var paramz = new
                {
                    ApprenticeshipQASubmissionId = apprenticeshipQASubmissionId,
                    app.ApprenticeshipId,
                    app.ApprenticeshipMarketingInformation,
                    app.ApprenticeshipTitle
                };

                return transaction.Connection.ExecuteAsync(sql, paramz, transaction);
            }
        }
    }
}
