using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetPostcodeInfosHandler : ISqlQueryHandler<GetPostcodeInfos, IDictionary<string, PostcodeInfo>>
    {
        public async Task<IDictionary<string, PostcodeInfo>> Execute(SqlTransaction transaction, GetPostcodeInfos query)
        {
            var sql = @"
SELECT pc.Postcode, pc.Position.Lat Latitude, pc.Position.Long Longitude, pc.InEngland
FROM Pttcd.Postcodes pc
JOIN @Postcodes x ON pc.Postcode = x.Value";

            var paramz = new
            {
                Postcodes = TvpHelper.CreateStringTable(query.Postcodes)
            };

            return (await transaction.Connection.QueryAsync<PostcodeInfo>(sql, paramz, transaction: transaction))
                .ToDictionary(p => p.Postcode, p => p);
        }
    }
}
