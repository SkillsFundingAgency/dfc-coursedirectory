using System.Data.SqlClient;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpsertLarsValidityHandler : ISqlQueryHandler<UpsertLarsValidity, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, UpsertLarsValidity query)
        {
            await UpsertHelper.Upsert(
                transaction,
                query.Records,
                keyPropertyNames: new[]
                {
                    nameof(UpsertLarsValidityRecord.ValidityCategory),
                    nameof(UpsertLarsValidityRecord.LearnAimRef),
                    nameof(UpsertLarsValidityRecord.LastNewStartDate)
                },
                "LARS.Validity");

            return new None();
        }
    }
}
