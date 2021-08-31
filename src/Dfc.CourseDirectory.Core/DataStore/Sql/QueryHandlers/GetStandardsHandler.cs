using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetStandardsHandler : ISqlQueryHandler<GetStandards, IDictionary<(int StandardCode, int StandardVersion), Standard>>
    {
        public async Task<IDictionary<(int StandardCode, int StandardVersion), Standard>> Execute(
            SqlTransaction transaction,
            GetStandards query)
        {
            var sql = @"
SELECT s.StandardCode, s.[Version], s.StandardName, s.NotionalEndLevel NotionalNVQLevelv2, s.OtherBodyApprovalRequired
FROM Pttcd.Standards s
JOIN @StandardCodes x ON s.StandardCode = x.StandardCode AND s.[Version] = x.[Version]";

            var paramz = new
            {
                StandardCodes = TvpHelper.CreateStandardCodesTable(query.StandardCodes)
            };

            return (await transaction.Connection.QueryAsync<Standard>(sql, paramz, transaction))
                .ToDictionary(s => (s.StandardCode, s.Version), s => s);
        }
    }
}
