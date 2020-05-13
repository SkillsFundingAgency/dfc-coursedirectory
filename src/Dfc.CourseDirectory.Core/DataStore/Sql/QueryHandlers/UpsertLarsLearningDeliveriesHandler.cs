using System.Data.SqlClient;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpsertLarsLearningDeliveriesHandler : ISqlQueryHandler<UpsertLarsLearningDeliveries, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, UpsertLarsLearningDeliveries query)
        {
            await UpsertHelper.Upsert(
                transaction,
                query.Records,
                keyPropertyNames: new[] { nameof(UpsertLarsLearningDeliveriesRecord.LearnAimRef) },
                "LARS.LearningDelivery");

            return new None();
        }
    }
}
