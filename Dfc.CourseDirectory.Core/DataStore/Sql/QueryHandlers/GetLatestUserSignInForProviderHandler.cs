using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLatestUserSignInForProviderHandler :
        ISqlQueryHandler<GetLatestUserSignInForProvider, LatestUserSignInForProviderResult>
    {
        public async Task<LatestUserSignInForProviderResult> Execute(
            SqlTransaction transaction,
            GetLatestUserSignInForProvider query)
        {
            var sql = @"
SELECT TOP 1
    s.SignedInUtc,
    u.UserId,
    u.Email,
    u.FirstName,
    u.LastName
FROM Pttcd.UserSignIns s
JOIN Pttcd.Users u ON s.UserId = u.UserId
JOIN Pttcd.UserProviders up ON u.UserId = up.UserId
WHERE up.ProviderId = @ProviderId
ORDER BY s.SignedInUtc DESC";

            return (await transaction.Connection.QueryAsync<DateTime, UserInfo, LatestUserSignInForProviderResult>(
                sql,
                (t, user) => new LatestUserSignInForProviderResult()
                {
                    SignedInUtc = t,
                    User = user
                },
                query,
                transaction,
                splitOn: "UserId")).SingleOrDefault();
        }
    }
}
