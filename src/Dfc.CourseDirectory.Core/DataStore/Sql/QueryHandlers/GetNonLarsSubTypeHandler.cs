using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetNonLarsSubTypeHandler : ISqlQueryHandler<GetNonLarsSubType,NonLarsSubType>
    {
        public async Task<NonLarsSubType> Execute(SqlTransaction transaction, GetNonLarsSubType query)
        {
            const string sql = @"
                        SELECT      NonLarsSubTypeId, Name, AddedOn, UpdatedOn, IsActive
                        FROM        [Pttcd].[NonLarsSubType]
                        WHERE       IsActive = 1 AND NonLarsSubTypeId = @NonLarsSubTypeId
                        ORDER BY    Name ASC";
            var paramz = new
            {
                query.NonLarsSubTypeId
            };

            return (await transaction.Connection.QueryAsync<NonLarsSubType>(sql, paramz, transaction)).First();
        }
    }
}
