using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetTLevelDefinitionsForProviderHandler : ISqlQueryHandler<GetTLevelDefinitionsForProvider, IReadOnlyCollection<TLevelDefinition>>
    {
        public async Task<IReadOnlyCollection<TLevelDefinition>> Execute(SqlTransaction transaction, GetTLevelDefinitionsForProvider query)
        {
            const string sql = @"
SELECT      d.TLevelDefinitionId, d.FrameworkCode, d.ProgType, d.QualificationLevel, d.Name
FROM        Pttcd.TLevelDefinitions d
INNER JOIN  Pttcd.ProviderTLevelDefinitions pd ON pd.TLevelDefinitionId = d.TLevelDefinitionId
WHERE       pd.ProviderId = @ProviderId
ORDER BY    d.Name ASC";

            return (await transaction.Connection.QueryAsync<TLevelDefinition>(sql, new { query.ProviderId }, transaction)).ToList();
        }
    }
}
