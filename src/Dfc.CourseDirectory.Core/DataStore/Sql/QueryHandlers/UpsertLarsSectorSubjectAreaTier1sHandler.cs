using System.Data.SqlClient;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpsertLarsSectorSubjectAreaTier1sHandler : ISqlQueryHandler<UpsertLarsSectorSubjectAreaTier1s, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, UpsertLarsSectorSubjectAreaTier1s query)
        {
            await UpsertHelper.Upsert(
                transaction,
                query.Records,
                keyPropertyNames: new[] { nameof(UpsertLarsSectorSubjectAreaTier1sRecord.SectorSubjectAreaTier1) },
                "LARS.SectorSubjectAreaTier1");

            return new None();
        }
    }
}
