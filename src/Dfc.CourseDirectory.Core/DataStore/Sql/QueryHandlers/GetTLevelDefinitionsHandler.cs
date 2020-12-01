using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetTLevelDefinitionsHandler : ISqlQueryHandler<GetTLevelDefinitions, IReadOnlyCollection<TLevelDefinition>>
    {
        public async Task<IReadOnlyCollection<TLevelDefinition>> Execute(SqlTransaction transaction, GetTLevelDefinitions query)
        {
            const string sql = @"
SELECT      TLevelDefinitionId, FrameworkCode, ProgType, Name
FROM        Pttcd.TLevelDefinitions
ORDER BY    Name ASC";

            return (await transaction.Connection.QueryAsync<TLevelDefinition>(sql, null, transaction)).ToList();
        }
    }
}
