using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class SetProviderNonLarsSubTypesHandler :
        ISqlQueryHandler<SetProviderNonLarsSubTypes, (IReadOnlyCollection<Guid> Added, IReadOnlyCollection<Guid> Removed)>
    {
        public async Task<(IReadOnlyCollection<Guid> Added, IReadOnlyCollection<Guid> Removed)> Execute(
            SqlTransaction transaction,
            SetProviderNonLarsSubTypes query)
        {
            const string sql = @"
DECLARE @AmendedProviderNonLarsSubTypes TABLE (
    NonLarsSubTypeId UNIQUEIDENTIFIER,
    Action nvarchar(10)
)

MERGE Pttcd.ProviderNonLarsSubType AS target
USING (
    SELECT
        Id NonLarsSubTypeId
    FROM @NonLarsSubTypes
) AS source
ON target.ProviderId = @ProviderId
AND target.NonLarsSubTypeId = source.NonLarsSubTypeId
WHEN NOT MATCHED THEN
    INSERT (
        ProviderId,
        NonLarsSubTypesId
    ) VALUES (
        @ProviderId,
        source.NonLarsSubTypeId
    )
WHEN NOT MATCHED BY SOURCE AND target.ProviderId = @ProviderId THEN
    DELETE
OUTPUT
    ISNULL(deleted.NonLarsSubTypeId, inserted.NonLarsSubTypeId), $action INTO @AmendedProviderNonLarsSubTypes;

SELECT * FROM @AmendedProviderNonLarsSubTypes";

            var changes = (await transaction.Connection.QueryAsync<Result>(
                sql,
                param: new
                {
                    query.ProviderId,
                    NonLarsSubTypeIds = TvpHelper.CreateGuidIdTable(query.NonLarsSubTypeIds)
                },
                transaction: transaction)).AsList();

            var added = changes.Where(r => r.Action == "INSERT").Select(r => r.NonLarsSubTypeId).ToArray();
            var removed = changes.Where(r => r.Action == "DELETE").Select(r => r.NonLarsSubTypeId).ToArray();

            return (added, removed);
        }

        private class Result
        {
            public Guid NonLarsSubTypeId { get; set; }
            public string Action { get; set; }
        }
    }
}
