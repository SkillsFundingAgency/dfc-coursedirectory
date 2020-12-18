using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class DeleteTLevelsForProviderWithTLevelDefinitionsHandler :
        ISqlQueryHandler<DeleteTLevelsForProviderWithTLevelDefinitions, None>
    {
        public async Task<None> Execute(
            SqlTransaction transaction,
            DeleteTLevelsForProviderWithTLevelDefinitions query)
        {
            var sql = @"
UPDATE Pttcd.TLevels SET
    TLevelStatus = @DeletedTLevelStatus,
    DeletedOn = @DeletedOn,
    DeletedByUserId = @DeletedByUserId
FROM Pttcd.TLevels t
JOIN @TLevelDefinitionIds x ON t.TLevelDefinitionId = x.Id
WHERE t.ProviderId = @ProviderId
AND t.TLevelStatus = @LiveTLevelStatus";

            await transaction.Connection.ExecuteAsync(
                sql,
                new
                {
                    query.ProviderId,
                    query.DeletedOn,
                    DeletedByUserId = query.DeletedBy.UserId,
                    TLevelDefinitionIds = TvpHelper.CreateGuidIdTable(query.TLevelDefinitionIds),
                    LiveTLevelStatus = TLevelStatus.Live,
                    DeletedTLevelStatus = TLevelLocationStatus.Deleted
                },
                transaction: transaction);

            return new None();
        }
    }
}
