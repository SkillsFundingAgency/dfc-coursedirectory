using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLatestApprenticeshipQAUnableToCompleteInfoForProviderHandler :
        ISqlQueryHandler<GetLatestApprenticeshipQAUnableToCompleteInfoForProvider, ApprenticeshipQAUnableToCompleteInfo>
    {
        public async Task<ApprenticeshipQAUnableToCompleteInfo> Execute(
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

            return (await transaction.Connection.QueryAsync<ApprenticeshipQAUnableToCompleteInfo, UserInfo, ApprenticeshipQAUnableToCompleteInfo>(
                sql,
                (r, u) =>
                {
                    r.AddedByUser = u;
                    return r;
                },
                query,
                transaction,
                splitOn: "UserId")).SingleOrDefault();
        }
    }
}
