using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetProviderContactByIdHandler : ISqlQueryHandler<GetProviderContactById, ProviderContact>
    {
        public Task<ProviderContact> Execute(SqlTransaction transaction, GetProviderContactById query)
        {
            var sql = @$"
            SELECT [ProviderContactId]
                  ,[ProviderId]
                  ,[ProviderContactIndex]
                  ,[ContactType]
                  ,[ContactRole]
                  ,[AddressSaonDescription]
                  ,[AddressPaonDescription]
                  ,[AddressStreetDescription]
                  ,[AddressLocality]
                  ,[AddressItems]
                  ,[AddressPostTown]
                  ,[AddressCounty]
                  ,[AddressPostcode]
                  ,[PersonalDetailsPersonNameTitle]
                  ,[PersonalDetailsPersonNameGivenName]
                  ,[PersonalDetailsPersonNameFamilyName]
                  ,[Telephone1]
                  ,[Telephone2]
                  ,[Fax]
                  ,[WebsiteAddress]
                  ,[Email]
              FROM [Pttcd].[ProviderContacts]
            WHERE {nameof(Provider.ProviderId)} = @{nameof(query.ProviderId)}";

            return transaction.Connection.QuerySingleOrDefaultAsync<ProviderContact>(sql, query, transaction);
        }
    }
}
