using System.Data.SqlClient;
using System.Net;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpdateProviderOnboardedHandler : ISqlQueryHandler<UpdateProviderOnboarded, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(SqlTransaction transaction, UpdateProviderOnboarded query)
        {


            var sql = $@"UPDATE Pttcd.Providers
                         SET ProviderStatus = @ProviderStatus,
                            UpdatedOn = @UpdatedOn,
                            UpdatedBy = @UpdatedBy
                        WHERE ProviderId = @ProviderId ";

            var paramz = new
            {
                ProviderStatus = ProviderStatus.Onboarded,
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
