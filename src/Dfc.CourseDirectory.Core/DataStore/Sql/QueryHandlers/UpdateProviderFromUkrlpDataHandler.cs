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
            var sqlProvider = $@"UPDATE [Pttcd].[Providers]
                            SET [ProviderName] = @ProviderName,
                            [Alias] = @Alias,
                            [UkrlpProviderStatusDescription] = @ProviderStatus,
                            [UpdatedOn] = @UpdatedOn,
                            [UpdatedBy] = @UpdatedBy
                        WHERE [ProviderId] = @ProviderId;";
            var sqlProviderContact = $@"
            UPDATE [Pttcd].[ProviderContacts] 
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
                              WHERE [ProviderId] = @ProviderId;
            ";

            var providerContact = query.Contacts.FirstOrDefault();
            var paramz = new
            {
                query.ProviderName,
                query.Alias,
                query.ProviderStatus,
                UpdatedOn = query.DateUpdated,
                query.UpdatedBy,
                query.ProviderId
            };
            var updated = await transaction.Connection.ExecuteAsync(sqlProvider, paramz, transaction) == 1;

            if (updated)
            {
                if (providerContact != null)
                {
                    var paramzContacts = new
                    {
                        query.ProviderId,
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
                        providerContact.Email
                    };

                    await transaction.Connection.ExecuteAsync(sqlProviderContact, paramzContacts, transaction);

                }
                return new Success();
            }
            return new NotFound();

        }

        private enum Result { Success = 0, NotFound = 1 }
    }
}
