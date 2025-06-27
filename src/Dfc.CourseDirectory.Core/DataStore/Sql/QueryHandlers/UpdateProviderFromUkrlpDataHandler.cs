using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;
using System.Data.SqlClient;
using Dapper;
using Dfc.CourseDirectory.Core.ReferenceData.Ukrlp;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpdateProviderFromUkrlpDataHandler : ISqlQueryHandler<UpdateProviderFromUkrlpData, Success>
    {
        private readonly ILogger<UkrlpSyncHelper> _logger;

        public UpdateProviderFromUkrlpDataHandler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UkrlpSyncHelper>();
        }
        
        public async Task<Success> Execute(SqlTransaction transaction, UpdateProviderFromUkrlpData query)
        {
            var sqlProvider = 
                $@"UPDATE [Pttcd].[Providers]
                    SET [ProviderName] = @ProviderName
                        ,[Alias] = @Alias
                        ,[UkrlpProviderStatusDescription] = @ProviderStatus
                        ,[UpdatedOn] = @UpdatedOn
                        ,[UpdatedBy] = @UpdatedBy
                    WHERE [ProviderId] = @ProviderId;
                ";
            
            var sqlProviderContact = 
                $@"UPDATE [Pttcd].[ProviderContacts] 
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
                    WHERE [ProviderId] = @ProviderId  AND  ContactType = 'P';
                ";

            var providerDataObj = new
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
                _logger.LogInformation("Update [Pttcd].[Providers] starting...");
                await transaction.Connection.ExecuteAsync(sqlProvider, providerDataObj, transaction);
                _logger.LogInformation("Update [Pttcd].[Providers] finished!");
            }

            var providerContact = query?.Contact;

            if (providerContact != null)
            {
                var providerContactDataObj = new
                {
                    ContactType = providerContact.ContactType,
                    AddressSaonDescription = providerContact.AddressSaonDescription,
                    AddressPaonDescription = providerContact.AddressPaonDescription,
                    AddressStreetDescription = providerContact.AddressStreetDescription,
                    AddressLocality = providerContact.AddressLocality,
                    AddressItems = providerContact.AddressItems,
                    AddressPostTown = providerContact.AddressPostTown,
                    AddressCounty = providerContact.AddressCounty,
                    AddressPostcode = providerContact.AddressPostcode,
                    PersonalDetailsPersonNameTitle = providerContact.PersonalDetailsPersonNameTitle,
                    PersonalDetailsPersonNameGivenName = providerContact.PersonalDetailsPersonNameGivenName,
                    PersonalDetailsPersonNameFamilyName = providerContact.PersonalDetailsPersonNameFamilyName,
                    Telephone1 = providerContact.Telephone1,
                    Fax = providerContact.Fax,
                    WebsiteAddress = providerContact.WebsiteAddress,
                    Email = providerContact.Email,
                    ProviderId = query.ProviderId
                };
                
                if (query.UpdateProviderContact)
                {
                    _logger.LogInformation("Update [Pttcd].[ProviderContacts] starting...");
                    await transaction.Connection.ExecuteAsync(sqlProviderContact, providerContactDataObj, transaction);
                    _logger.LogInformation("Update [Pttcd].[ProviderContacts] finished!");
                }
            }
            
            return new Success();
        }
    }
}
