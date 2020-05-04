using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

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

            async Task CreateSubmissionApprenticeship(CreateApprenticeshipQASubmissionApprenticeship app)
            {
                var sql = @"
INSERT INTO Pttcd.ApprenticeshipQASubmissionApprenticeships
(ApprenticeshipQASubmissionId, ApprenticeshipId, ApprenticeshipTitle, ApprenticeshipMarketingInformation)
VALUES (@ApprenticeshipQASubmissionId, @ApprenticeshipId, @ApprenticeshipTitle, @ApprenticeshipMarketingInformation)

SELECT SCOPE_IDENTITY() AS ApprenticeshipQASubmissionApprenticeshipId";

                var paramz = new
                {
                    ApprenticeshipQASubmissionId = apprenticeshipQASubmissionId,
                    app.ApprenticeshipId,
                    app.ApprenticeshipMarketingInformation,
                    app.ApprenticeshipTitle
                };

                var apprenticeshipQASubmissionApprenticeshipId = 
                    await transaction.Connection.QuerySingleAsync<int>(sql, paramz, transaction);

                foreach (var location in app.Locations)
                {
                    await CreateSubmissionApprenticeshipLocation(apprenticeshipQASubmissionApprenticeshipId, location);
                }
            }

            Task CreateSubmissionApprenticeshipLocation(
                int apprenticeshipQASubmissionApprenticeshipId,
                CreateApprenticeshipQASubmissionApprenticeshipLocation location)
            {
                var sql = @"
INSERT INTO Pttcd.ApprenticeshipQASubmissionApprenticeshipLocations
(ApprenticeshipQASubmissionApprenticeshipId, ApprenticeshipLocationType, [National], RegionIds, VenueName, DeliveryModes, Radius)
VALUES (@ApprenticeshipQASubmissionApprenticeshipId, @ApprenticeshipLocationType, @National, @RegionIds, @VenueName, @DeliveryModes, @Radius)";

                var paramz = new
                {
                    ApprenticeshipQASubmissionApprenticeshipId = apprenticeshipQASubmissionApprenticeshipId,
                    ApprenticeshipLocationType = location.IsClassroomBased ? ApprenticeshipLocationType.ClassroomBased : ApprenticeshipLocationType.EmployerBased,
                    National = location.Match(c => (bool?)null, e => e.Value is National),
                    RegionIds = location.Match(c => null, e => e.Match(national => null, r => string.Join(",", r.SubRegionIds))),
                    VenueName = location.Match(c => c.VenueName, e => null),
                    DeliveryModes = location.Match(c => string.Join(",", c.DeliveryModes.Cast<int>()), e => null),
                    Radius = location.Match(c => c.Radius, e => (int?)null)
                };

                return transaction.Connection.ExecuteAsync(sql, paramz, transaction);
            }
        }
    }
}
