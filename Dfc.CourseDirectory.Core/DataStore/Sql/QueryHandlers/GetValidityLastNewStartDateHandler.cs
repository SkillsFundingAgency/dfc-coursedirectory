﻿using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetValidityLastNewStartDateHandler : ISqlQueryHandler<GetValidityLastNewStartDate, List<string>>
    {
        public async Task<List<string>> Execute(SqlTransaction transaction, GetValidityLastNewStartDate query)
        {
            var sql = $@"SELECT  * FROM [LARS].[Validity] WHERE LearnAimRef = @LearnAimRef";

            var paramz = new
            {
                query.LearnAimRef
            };

            var rows = (await transaction.Connection.QueryAsync<Validity>(sql, paramz, transaction: transaction)).AsList();

            return rows.Select(r=> r.LastNewStartDate).ToList();
        }
    }
}
