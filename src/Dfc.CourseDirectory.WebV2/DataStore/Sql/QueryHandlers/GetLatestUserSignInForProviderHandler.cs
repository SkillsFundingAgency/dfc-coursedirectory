﻿using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.QueryHandlers
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
    u.LastName,
    u.ProviderId
FROM Pttcd.UserSignIns s
JOIN Pttcd.Users u ON s.UserId = u.UserId
WHERE u.ProviderId = @ProviderId
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
