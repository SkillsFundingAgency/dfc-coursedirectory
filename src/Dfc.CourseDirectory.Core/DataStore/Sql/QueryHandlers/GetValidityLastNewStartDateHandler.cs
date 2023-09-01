using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetValidityLastNewStartDateHandler : ISqlQueryHandler<GetValidityLastNewStartDate, List<DateTime?>>
    {
        public async Task<List<DateTime?>> Execute(SqlTransaction transaction, GetValidityLastNewStartDate query)
        {
            var sql = $@"SELECT  * FROM [LARS].[Validity] WHERE LearnAimRef = @LearnAimRef";

            var rows = (await transaction.Connection.QueryAsync<Validity>(sql, transaction: transaction)).AsList();

            return rows.Select(r=> r.LastNewStartDate).ToList();
        }
    }
}
