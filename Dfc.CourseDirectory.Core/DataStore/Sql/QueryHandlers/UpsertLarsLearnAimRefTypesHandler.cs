using System.Data.SqlClient;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpsertLarsLearnAimRefTypesHandler : ISqlQueryHandler<UpsertLarsLearnAimRefTypes, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, UpsertLarsLearnAimRefTypes query)
        {
            await UpsertHelper.Upsert(
                transaction,
                query.Records,
                keyPropertyNames: new[] { nameof(UpsertLarsLearnAimRefTypesRecord.LearnAimRefType) },
                "LARS.LearnAimRefType");

            return new None();
        }
    }
}
