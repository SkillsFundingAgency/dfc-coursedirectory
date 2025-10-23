using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLarsCourseTypeHandler : ISqlQueryHandler<GetLarsCourseType, IReadOnlyCollection<LarsCourseType>>
    {
        public async Task<IReadOnlyCollection<LarsCourseType>> Execute(SqlTransaction transaction, GetLarsCourseType query)
        {
            var sql = $@"SELECT ldc.LearnAimRef, ldc.CategoryRef, ctc.CourseType, ld.LearnAimRefTitle
FROM LARS.LearningDeliveryCategory ldc
LEFT JOIN Pttcd.CourseTypeCategory ctc ON ctc.CategoryRef = ldc.CategoryRef
INNER JOIN LARS.LearningDelivery ld ON ld.LearnAimRef = ldc.LearnAimRef
WHERE ldc.LearnAimRef = @LearnAimRef";

            var param = new
            {
                LearnAimRef = query.LearnAimRef
            };

            return (await transaction.Connection.QueryAsync<LarsCourseType>(sql, param, transaction))
                .AsList();
        }
    }
}
