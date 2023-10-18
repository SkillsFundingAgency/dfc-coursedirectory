using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetProviderByUkprnHandler : ISqlQueryHandler<GetProviderByUkprn, Provider>
    {
        public Task<Provider> Execute(SqlTransaction transaction, GetProviderByUkprn query)
        {
            var sql = @$"
SELECT  ProviderId,
        Ukprn,
        ProviderName,
        ProviderType,
        Alias,
        MarketingInformation,
        ProviderStatus as Status,
        UkrlpProviderStatusDescription as ProviderStatus,
        DisplayNameSource,
        LearnerSatisfaction,
        EmployerSatisfaction
FROM    Pttcd.Providers
WHERE   Ukprn = @{nameof(query.Ukprn)}";

            return transaction.Connection.QuerySingleOrDefaultAsync<Provider>(sql, query, transaction);
        }
    }
}
