using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetProviderByIdHandler : ISqlQueryHandler<GetProviderById, Provider>
    {
        public Task<Provider> Execute(SqlTransaction transaction, GetProviderById query)
        {
            var sql = @$"
SELECT  ProviderId,
        Ukprn,
        ProviderName,
        ProviderType,
        Alias,
        DisplayNameSource,
        ApprenticeshipQAStatus,
        LearnerSatisfaction,
        EmployerSatisfaction
FROM    Pttcd.Providers
WHERE   {nameof(Provider.ProviderId)} = @{nameof(query.ProviderId)}";

            return transaction.Connection.QuerySingleOrDefaultAsync<Provider>(sql, query, transaction);
        }
    }
}
