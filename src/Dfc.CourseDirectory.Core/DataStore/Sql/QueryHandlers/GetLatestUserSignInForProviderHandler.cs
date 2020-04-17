using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLatestUserSignInForProviderHandler
        : ISqlQueryHandler<GetLatestUserSignInForProvider, OneOf<None, LatestUserSignInForProviderResult>>
    {
        public async Task<OneOf<None, LatestUserSignInForProviderResult>> Execute(
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

            var result = (await transaction.Connection.QueryAsync<DateTime, UserInfo, LatestUserSignInForProviderResult>(
                sql,
                (t, user) => new LatestUserSignInForProviderResult()
                {
                    SignedInUtc = t,
                    User = user
                },
                query,
                transaction,
                splitOn: "UserId")).SingleOrDefault();

            if (result == null)
            {
                return new None();
            }
            else
            {
                return result;
            }
        }
    }
}
