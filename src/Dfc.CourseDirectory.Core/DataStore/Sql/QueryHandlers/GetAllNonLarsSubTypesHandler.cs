using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetAllNonLarsSubTypesHandler : ISqlQueryHandler<GetAllNonLarsSubTypes, IReadOnlyCollection<NonLarsSubType>>
    {
        public async Task<IReadOnlyCollection<NonLarsSubType>> Execute(SqlTransaction transaction, GetAllNonLarsSubTypes query)
        {
            const string sql = @"
                        SELECT      NonLarsSubTypeId, Name, AddedOn, UpdatedOn, IsActive
                        FROM        [Pttcd].[NonLarsSubType]
                        WHERE       IsActive = 1
                        ORDER BY    Name ASC";

            return (await transaction.Connection.QueryAsync<NonLarsSubType>(sql, new { query }, transaction)).ToList();
        }
    }
}
