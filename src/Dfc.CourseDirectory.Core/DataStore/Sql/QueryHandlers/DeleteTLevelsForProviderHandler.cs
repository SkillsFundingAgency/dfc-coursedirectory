using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class DeleteTLevelsForProviderHandler : ISqlQueryHandler<DeleteTLevelsForProvider, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, DeleteTLevelsForProvider query)
        {
            var sql = $@"
UPDATE [Pttcd].[TLevelLocations] 
SET TLevelLocationStatus = {(int)TLevelStatus.Deleted} 
FROM [Pttcd].[TLevelLocations] l 
INNER JOIN [Pttcd].[TLevels] t ON l.TLevelId = t.TLevelId 
WHERE t.ProviderId = @ProviderId 
AND l.TLevelLocationStatus <> {(int)TLevelStatus.Deleted}

DECLARE @TLevelIds Pttcd.GuidIdTable
DECLARE @DeletedOn DATETIME
SET @DeletedOn = GETDATE()

UPDATE [Pttcd].[TLevels] 
SET TLevelStatus = {(int)TLevelStatus.Deleted}, UpdatedOn = GETDATE(), DeletedOn = GETDATE() 
OUTPUT INSERTED.TLevelId INTO @TLevelIds
WHERE ProviderId = @ProviderId 
AND TLevelStatus <> {(int)TLevelStatus.Deleted}

EXEC Pttcd.RefreshFindACourseIndexForTLevels @TLevelIds, @DeletedOn
";

            var paramz = new
            {
                query.ProviderId,
            };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new Success();
        }
    }
}
