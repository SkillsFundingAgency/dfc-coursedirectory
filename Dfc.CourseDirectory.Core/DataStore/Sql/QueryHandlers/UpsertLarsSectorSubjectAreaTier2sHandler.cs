using System.Data.SqlClient;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpsertLarsSectorSubjectAreaTier2sHandler : ISqlQueryHandler<UpsertLarsSectorSubjectAreaTier2s, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, UpsertLarsSectorSubjectAreaTier2s query)
        {
            await UpsertHelper.Upsert(
                transaction,
                query.Records,
                keyPropertyNames: new[] { nameof(UpsertLarsSectorSubjectAreaTier2sRecord.SectorSubjectAreaTier2) },
                "LARS.SectorSubjectAreaTier2");

            return new None();
        }
    }
}
