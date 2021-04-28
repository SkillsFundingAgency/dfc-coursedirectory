using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetFeChoicesByProviderUkprnsHandler : ISqlQueryHandler<GetFeChoicesByProviderUkprns, IReadOnlyDictionary<int, FeChoice>>
    {
        public async Task<IReadOnlyDictionary<int, FeChoice>> Execute(SqlTransaction transaction, GetFeChoicesByProviderUkprns query)
        {
            var sql = @"
SELECT ID, Ukprn, LearnerSatisfaction, 
EmployerSatisfaction, CreatedDateTimeUtc, CreatedOn, 
CreatedBy, LastUpdatedBy, LastUpdatedOn 
FROM[Pttcd].[FeChoices]
JOIN @ProviderUkprns x ON v.Ukprn = x.Id";

            var param = new
            {
                ProviderUkprns = TvpHelper.CreateIntIdTable(query.ProviderUkprns)
            };

            return (await transaction.Connection.QueryAsync<FeChoice>(sql, param, transaction))
                .ToDictionary(v => v.UKPRN, v => v);
        }
    }
}
