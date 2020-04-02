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
    public class GetLatestApprenticeshipQAUnableToCompleteInfoForProviderHandler
        : ISqlQueryHandler<GetLatestApprenticeshipQAUnableToCompleteInfoForProvider, OneOf<None, ApprenticeshipQAUnableToCompleteInfo>>
    {
        public async Task<OneOf<None, ApprenticeshipQAUnableToCompleteInfo>> Execute(
            SqlTransaction transaction,
            GetLatestApprenticeshipQAUnableToCompleteInfoForProvider query)
        {
            var sql = @"
SELECT TOP 1
    r.ProviderId,
    r.UnableToCompleteReasons,
    r.Comments,
    r.AddedOn, 
    u.UserId,
    u.Email,
    u.FirstName,
    u.LastName
FROM Pttcd.ApprenticeshipQAUnableToCompleteInfo r
JOIN Pttcd.Users u ON r.AddedByUserId = u.UserId
WHERE r.ProviderId = @ProviderId
ORDER BY r.AddedOn DESC";

            var result = (await transaction.Connection.QueryAsync<ApprenticeshipQAUnableToCompleteInfo, UserInfo, ApprenticeshipQAUnableToCompleteInfo>(
                sql,
                (r, u) =>
                {
                    r.AddedByUser = u;
                    return r;
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
