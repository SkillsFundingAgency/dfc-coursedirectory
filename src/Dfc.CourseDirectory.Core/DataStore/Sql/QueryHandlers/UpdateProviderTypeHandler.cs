using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf;
using OneOf.Types;
using System.Data.SqlClient;
using Dapper;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpdateProviderTypeHandler : ISqlQueryHandler<UpdateProviderType, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(
            SqlTransaction transaction,
            UpdateProviderType query)
        {
            var sql = $@"UPDATE [Pttcd].[Providers]
                         SET [ProviderType] = @ProviderType,
                            [UpdatedOn] = @UpdatedOn,
                            [UpdatedBy] = @UpdatedBy
                        WHERE [ProviderId] = @ProviderId ";

            var paramz = new
            {
                query.ProviderType,
                query.UpdatedOn,
                UpdatedBy = query.UpdatedBy.UserId,
                query.ProviderId
            };
            var updated = await transaction.Connection.ExecuteAsync(sql, paramz, transaction) == 1;

            if (updated)
            {
                return new Success();
            }
            else
            {
                return new NotFound();
            }
        }

    }
}
