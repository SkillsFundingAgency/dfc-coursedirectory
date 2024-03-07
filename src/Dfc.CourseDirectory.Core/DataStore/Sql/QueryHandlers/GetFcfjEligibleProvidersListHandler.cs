using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetFcfjEligibleProvidersListHandler : ISqlQueryHandler<GetFcfjEligibleProvidersList, IReadOnlyCollection<Guid>>
    {
        public async Task<IReadOnlyCollection<Guid>> Execute(SqlTransaction transaction, GetFcfjEligibleProvidersList query)
        {
            var sql = $@"SELECT DISTINCT ProviderId FROM Pttcd.FindACourseIndexCampaignCodes WHERE CampaignCodesJson LIKE '%LEVEL3_FREE%'";            

            var records = (await transaction.Connection.QueryAsync<Guid>(sql, transaction: transaction)).AsList();

            return records;
        }
    }
}
