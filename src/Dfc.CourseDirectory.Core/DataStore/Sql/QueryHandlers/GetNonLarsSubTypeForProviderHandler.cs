using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetNonLarsSubTypeForProviderHandler : ISqlQueryHandler<GetNonLarsSubTypeForProvider, IReadOnlyCollection<NonLarsSubType>>
    {
        public async Task<IReadOnlyCollection<NonLarsSubType>> Execute(SqlTransaction transaction, GetNonLarsSubTypeForProvider query)
        {
            const string sql = @"
                        SELECT      nst.NonLarsSubTypeId, Name, AddedOn, UpdatedOn, IsActive
                            FROM        [Pttcd].[NonLarsSubType] nst
                            INNER JOIN [Pttcd].[ProviderNonLarsSubType] pnst ON nst.NonLarsSubTypeId = pnst.NonLarsSubTypeId
                        WHERE       nst.IsActive = 1 and pnst.ProviderId = @ProviderId
                        ORDER BY    Name ASC";

            return (await transaction.Connection.QueryAsync<NonLarsSubType>(sql, new { query.ProviderId }, transaction)).ToList();
        }
    }
}
