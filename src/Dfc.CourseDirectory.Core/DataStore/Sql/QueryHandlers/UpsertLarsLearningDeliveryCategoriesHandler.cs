using System.Data.SqlClient;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpsertLarsLearningDeliveryCategoriesHandler : ISqlQueryHandler<UpsertLarsLearningDeliveryCategories, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, UpsertLarsLearningDeliveryCategories query)
        {
            await UpsertHelper.Upsert(
                transaction,
                query.Records,
                keyPropertyNames: new[]
                {
                    nameof(UpsertLarsLearningDeliveryCategoriesRecord.CategoryRef),
                    nameof(UpsertLarsLearningDeliveryCategoriesRecord.LearnAimRef),
                    nameof(UpsertLarsLearningDeliveryCategoriesRecord.EffectiveFrom)
                },
                "LARS.LearningDeliveryCategory");

            return new None();
        }
    }
}
