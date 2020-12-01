using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpsertProviderTLevelDefinitionsHandler : ISqlQueryHandler<UpsertProviderTLevelDefinitions, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, UpsertProviderTLevelDefinitions query)
        {
            const string createTableSql = @"
CREATE TABLE #TLevelDefinitionIds (
    TLevelDefinitionId UNIQUEIDENTIFIER
)";

            await transaction.Connection.ExecuteAsync(createTableSql, transaction: transaction);

            await BulkCopyHelper.WriteRecords(
                query.TLevelDefinitionIds.Distinct().Select(id => new { TLevelDefinitionId = id }),
                tableName: "#TLevelDefinitionIds",
                transaction);

            const string sql = @"
MERGE Pttcd.ProviderTLevelDefinitions AS target
USING (
    SELECT
        TLevelDefinitionId
    FROM #TLevelDefinitionIds
) AS source
ON target.ProviderId = @ProviderId
AND target.TLevelDefinitionId = source.TLevelDefinitionId
WHEN NOT MATCHED THEN
    INSERT (
        ProviderId,
        TLevelDefinitionId
    ) VALUES (
        @ProviderId,
        source.TLevelDefinitionId
    )
WHEN NOT MATCHED BY SOURCE THEN
    DELETE;";

            await transaction.Connection.ExecuteAsync(
                sql,
                param: new { query.ProviderId },
                transaction: transaction);

            return new None();
        }
    }
}
