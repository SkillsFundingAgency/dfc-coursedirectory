using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class PublishApprenticeshipHandler : ISqlQueryHandler<PublishApprenticeship, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(SqlTransaction transaction, PublishApprenticeship query)
        {
            var sql = $@"
UPDATE Pttcd.Apprenticeships SET
    ApprenticeshipStatus = {(int)ApprenticeshipStatus.Live},
    UpdatedOn = @PublishedOn,
    UpdatedBy = @PublishedByUserId
WHERE ApprenticeshipId = @ApprenticeshipId
AND ApprenticeshipStatus = {(int)ApprenticeshipStatus.Pending}";

            var paramz = new
            {
                query.ApprenticeshipId,
                query.PublishedOn,
                PublishedByUserId = query.PublishedBy.UserId
            };

            var published = await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            if (published == 1)
            {
                return new Success();
            }
            else
            {
                return new NotFound();
            }
        }
    }
}
