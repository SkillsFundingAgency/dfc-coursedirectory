using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetProvidersByIdsHandler : ISqlQueryHandler<GetProvidersByIds, IReadOnlyDictionary<Guid, Provider>>
    {
        public async Task<IReadOnlyDictionary<Guid, Provider>> Execute(SqlTransaction transaction, GetProvidersByIds query)
        {
            var sqlParameters = new
            {
                ProviderIds = TvpHelper.CreateGuidIdTable(query.ProviderIds)
            };

            var sql = @$"
SELECT  ProviderId,
        Ukprn,
        ProviderName,
        ProviderType,
        Alias,
        DisplayNameSource,
        BulkUploadInProgress,
        BulkUploadPublishInProgress,
        BulkUploadStartedDateTime,
        BulkUploadTotalRowCount,
        LearnerSatisfaction,
        EmployerSatisfaction
FROM    Pttcd.Providers
WHERE   {nameof(Provider.ProviderId)} IN (SELECT * FROM @{nameof(sqlParameters.ProviderIds)})";

            var results = await transaction.Connection.QueryAsync<Provider>(sql, sqlParameters, transaction);
            
            return results.ToDictionary(p => p.ProviderId, v => v);
        }
    }
}
