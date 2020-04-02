using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.QueryHandlers
{
    public class GetUserByIdHandler : ISqlQueryHandler<GetUserById, OneOf<NotFound, UserInfo>>
    {
        public async Task<OneOf<NotFound, UserInfo>> Execute(SqlTransaction transaction, GetUserById query)
        {
            var sql = @"
SELECT UserId, Email, FirstName, LastName
FROM Pttcd.Users
WHERE UserId = @UserId";

            var result = await transaction.Connection.QuerySingleOrDefaultAsync<UserInfo>(sql, query, transaction);

            if (result == null)
            {
                return new NotFound();
            }
            else
            {
                return result;
            }
        }
    }
}
