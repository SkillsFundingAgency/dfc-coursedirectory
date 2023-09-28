using System.Data.SqlClient;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using OneOf.Types;
using Dapper;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetProvidersHandler : ISqlQueryHandler<GetProviders, IList<ProviderUkprn>>
    {
        public async Task<IList<ProviderUkprn>> Execute(SqlTransaction transaction, GetProviders query)
        {

            var sql = $@"Select ProviderId,
                               Ukprn,
                               RowNum
                        From (SELECT [ProviderId]
                              ,[Ukprn]
                              ,ROW_NUMBER() OVER (ORDER BY ProviderId) AS RowNum
                          FROM [Pttcd].[Providers]) as Providers
                        Where RowNum BETWEEN {query.Min} and {query.Max}";

            var providerList = await transaction.Connection.QueryAsync(sql,transaction);

            return (IList<ProviderUkprn>)providerList;
        }
    }
}
