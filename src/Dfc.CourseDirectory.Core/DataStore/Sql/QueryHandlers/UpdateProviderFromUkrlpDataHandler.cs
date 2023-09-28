using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf;
using OneOf.Types;
using System.Data.SqlClient;
using Dapper;
using Dfc.CourseDirectory.Core.ReferenceData.Ukrlp;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpdateProviderFromUkrlpDataHandler :
        ISqlQueryHandler<UpdateProviderFromUkrlpData, Success>
    {
        private readonly ILogger<UkrlpSyncHelper> _logger;

        public UpdateProviderFromUkrlpDataHandler(ILoggerFactory loggerFactory)
        {
                    _logger = loggerFactory.CreateLogger<UkrlpSyncHelper>();

        }
    public async Task<Success> Execute(
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

            var providerContact = query?.Contact;
            var paramz = new
            {
                query.ProviderName,
                query.Alias,
                query.ProviderStatus,
                UpdatedOn = query.DateUpdated,
                query.UpdatedBy,
                query.ProviderId,
            };
            if (query.UpdateProvider)
            {
                _logger.LogInformation("Update Provider table starting...");
                await transaction.Connection.ExecuteAsync(sqlProvider, paramz, transaction);
                _logger.LogInformation("Update provider table finished!");
            }
            
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
                if (query.UpdateProviderContact)
                {
                    _logger.LogInformation("Update ProviderContacts table starting...");
                    await transaction.Connection.ExecuteAsync(sqlProviderContact, paramzContacts, transaction);
                    _logger.LogInformation("Update ProviderContacts table finishe!.");
                }
            }
            return new Success();
            

        }

    }
}
