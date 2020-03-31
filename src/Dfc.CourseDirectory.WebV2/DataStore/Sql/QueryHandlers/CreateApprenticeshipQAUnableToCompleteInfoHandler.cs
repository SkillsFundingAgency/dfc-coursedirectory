using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.QueryHandlers
{
    public class CreateApprenticeshipQAUnableToCompleteInfoHandler
        : ISqlQueryHandler<CreateApprenticeshipQAUnableToCompleteInfo, int>
    {
        public Task<int> Execute(SqlTransaction transaction, CreateApprenticeshipQAUnableToCompleteInfo query)
        {
            var sql = @"
INSERT INTO Pttcd.ApprenticeshipQAUnableToCompleteInfo
(ProviderId, UnableToCompleteReasons, Comments, StandardName, AddedOn, AddedByUserId)
VALUES (@ProviderId, @UnableToCompleteReasons, @Comments, @StandardName, @AddedOn, @AddedByUserId)

SELECT SCOPE_IDENTITY() ApprenticeshipQAUnableToCompleteId";

            return transaction.Connection.QuerySingleOrDefaultAsync<int>(sql, query, transaction);
        }
    }
}
