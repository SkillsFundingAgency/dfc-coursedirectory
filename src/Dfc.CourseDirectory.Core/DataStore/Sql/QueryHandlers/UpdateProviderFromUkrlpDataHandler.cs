using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
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
            var sql = $@"UPDATE [Pttcd].[Providers]
                            SET [ProviderName] = @ProviderName,
                            [Alias] = @Alias,
                            [ProviderStatus] = @ProviderStatus,
                            [UpdatedOn] = @UpdatedOn,
                            [UpdatedBy] = @UpdatedBy
                        WHERE [ProviderId] = @ProviderId ";


            var paramz = new
            {
                query.ProviderName,
                query.Alias,
                query.ProviderStatus,
                UpdatedOn = query.DateUpdated,
                query.UpdatedBy,
                query.ProviderId,
            };

            var result = await transaction.Connection.QuerySingleAsync<Result>(sql, paramz, transaction);

            if (result == Result.Success)
            {
                var pcsql = $@"UPDATE [Pttcd].[ProviderContacts] 
                                SET [ContactType] = @ContactType
                                  ,[AddressSaonDescription] = @AddressSaonDescription
                                  ,[AddressPaonDescription] = @AddressPaonDescription
                                  ,[AddressStreetDescription] = @AddressStreetDescription
                                  ,[AddressLocality] = @AddressLocality
                                  ,[AddressItems] = @AddressItems
                                  ,[AddressPostTown] = @AddressPostTown
                                  ,[AddressCounty] = @AddressCounty
                                  ,[AddressPostcode] = @AddressPostcode
                                  ,[PersonalDetailsPersonNameTitle] = @PersonalDetailsPersonNameTitle
                                  ,[PersonalDetailsPersonNameGivenName] = @PersonalDetailsPersonNameGivenName
                                  ,[PersonalDetailsPersonNameFamilyName] = @PersonalDetailsPersonNameFamilyName
                                  ,[Telephone1] = @Telephone1
                                  ,[Fax] = @Fax
                                  ,[WebsiteAddress] = @WebsiteAddress
                                  ,[Email] = @Email
                              WHERE [ProviderId] = @ProviderId; ";

                var providerContact = query.Contacts.FirstOrDefault();
                var pcparamz = new
                {
                    providerContact.ContactType,
                    providerContact.AddressSaonDescription,
                    providerContact.AddressPaonDescription,
                    providerContact.AddressStreetDescription,
                    providerContact.AddressLocality,
                    providerContact.AddressItems,
                    providerContact.AddressPostTown,
                    providerContact.AddressCounty,
                    providerContact.AddressPostcode,
                    providerContact.PersonalDetailsPersonNameTitle,
                    providerContact.PersonalDetailsPersonNameGivenName,
                    providerContact.PersonalDetailsPersonNameFamilyName,
                    providerContact.Telephone1,
                    providerContact.Fax,
                    providerContact.WebsiteAddress,
                    providerContact.Email,
                    query.ProviderId
                };

                result = await transaction.Connection.QuerySingleAsync<Result>(pcsql, pcparamz, transaction);

                if (result == Result.Success)
                {
                    return new Success();
                }
                return new NotFound();
            }
            return new NotFound();
        }

        private enum Result { Success = 0, NotFound = 1 }
    }
}
