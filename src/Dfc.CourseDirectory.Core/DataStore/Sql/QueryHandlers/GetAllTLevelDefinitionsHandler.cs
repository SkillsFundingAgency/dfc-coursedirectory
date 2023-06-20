using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetAllTLevelDefinitionsHandler : ISqlQueryHandler<GetAllTLevelDefinitions, IReadOnlyCollection<TLevelDefinition>>
    {
        public async Task<IReadOnlyCollection<TLevelDefinition>> Execute(SqlTransaction transaction, GetAllTLevelDefinitions query)
        {
            const string sql = @"
SELECT      d.TLevelDefinitionId, d.FrameworkCode, d.ProgType, d.QualificationLevel, d.Name
FROM        Pttcd.TLevelDefinitions d
ORDER BY    d.Name ASC";

            return (await transaction.Connection.QueryAsync<TLevelDefinition>(sql, new { query }, transaction)).ToList();
        }
    }
}
