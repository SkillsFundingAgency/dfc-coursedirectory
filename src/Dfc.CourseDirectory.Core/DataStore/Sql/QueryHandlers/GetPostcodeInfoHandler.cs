using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetPostcodeInfoHandler : ISqlQueryHandler<GetPostcodeInfo, PostcodeInfo>
    {
        public Task<PostcodeInfo> Execute(SqlTransaction transaction, GetPostcodeInfo query)
        {
            var sql = @"
SELECT Postcode, Position.Lat Latitude, Position.Long Longitude, InEngland FROM Pttcd.Postcodes
WHERE Postcode = @Postcode";

            return transaction.Connection.QuerySingleOrDefaultAsync<PostcodeInfo>(sql, new { query.Postcode }, transaction: transaction);
        }
    }
}
