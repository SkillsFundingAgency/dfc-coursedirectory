using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetSectorsHandler : ISqlQueryHandler<GetSectors, IReadOnlyCollection<Sector>>
    {
        public async Task<IReadOnlyCollection<Sector>> Execute(SqlTransaction transaction, GetSectors query)
        {
            var sql = @"SELECT Id, Code, [Description] 
                        FROM Pttcd.Sectors";

            var sectors = (await transaction.Connection.QueryAsync<Sector>(sql, transaction: transaction)).AsList();

            return sectors;
        }        
    }
}
