using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetCampaignCodesForProviderHandler : ISqlQueryHandler<GetCampaignCodesForProvider, IReadOnlyCollection<ProviderCampaignCode>>
    {
        public async Task<IReadOnlyCollection<ProviderCampaignCode>> Execute(SqlTransaction transaction, GetCampaignCodesForProvider query)
        {
            var sql = $@"
    SELECT
        [FindACourseIndexCampaignCodeId] as CodeId
        ,[ProviderId]
        ,[LearnAimRef]
        ,[CampaignCodesJson] as CampaignCodes
    FROM Pttcd.FindACourseIndexCampaignCodes
    WHERE ProviderId = @ProviderId
";

            var paramz = new
            {
                query.ProviderId
            };

            var records = (await transaction.Connection.QueryAsync<ProviderCampaignCode>(sql, paramz, transaction)).ToList();

            return records;
        }
    }
}
