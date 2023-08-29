using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using OneOf;
using OneOf.Types;
using System.Data.SqlClient;
using Dapper;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpdateProviderFromUkrlpDataHandler :
        ISqlQueryHandler<UpdateProviderFromUkrlpData, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(
            SqlTransaction transaction,
            UpdateProviderFromUkrlpData query)
        {
            var sql = $@"
UPDATE 
";


            var paramz = new
            {
                query.ProviderName,
                ProviderAliases = query.Aliases.ToList(),
                ProviderContacts = query.Contacts.ToList(),
                query.Alias,
                query.ProviderStatus,
                query.DateUpdated,
                query.UpdatedBy
            };

            var result = await transaction.Connection.QuerySingleAsync<Result>(sql, paramz, transaction);

            if (result == Result.Success)
            {
                return new Success();
            }
            else
            {
                return new NotFound();
            }
        }

        private enum Result { Success = 0, NotFound = 1 }
    }
}
