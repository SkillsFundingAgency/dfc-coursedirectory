using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class SetProviderTLevelDefinitionsHandler :
        ISqlQueryHandler<SetProviderTLevelDefinitions, (IReadOnlyCollection<Guid> Added, IReadOnlyCollection<Guid> Removed)>
    {
        public async Task<(IReadOnlyCollection<Guid> Added, IReadOnlyCollection<Guid> Removed)> Execute(
            SqlTransaction transaction,
            SetProviderTLevelDefinitions query)
        {
            const string sql = @"
DECLARE @AmendedProviderTLevelDefinitions TABLE (
    TLevelDefinitionId UNIQUEIDENTIFIER,
    Action nvarchar(10)
)

MERGE Pttcd.ProviderTLevelDefinitions AS target
USING (
    SELECT
        Id TLevelDefinitionId
    FROM @TLevelDefinitionIds
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
WHEN NOT MATCHED BY SOURCE AND target.ProviderId = @ProviderId THEN
    DELETE
OUTPUT
    ISNULL(deleted.TLevelDefinitionId, inserted.TLevelDefinitionId), $action INTO @AmendedProviderTLevelDefinitions;

SELECT * FROM @AmendedProviderTLevelDefinitions";

            var changes = (await transaction.Connection.QueryAsync<Result>(
                sql,
                param: new
                {
                    query.ProviderId,
                    TLevelDefinitionIds = TvpHelper.CreateGuidIdTable(query.TLevelDefinitionIds)
                },
                transaction: transaction)).AsList();

            var added = changes.Where(r => r.Action == "INSERT").Select(r => r.TLevelDefinitionId).ToArray();
            var removed = changes.Where(r => r.Action == "DELETE").Select(r => r.TLevelDefinitionId).ToArray();

            return (added, removed);
        }

        private class Result
        {
            public Guid TLevelDefinitionId { get; set; }
            public string Action { get; set; }
        }
    }
}
