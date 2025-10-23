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
            var updateProviderSql = $@"UPDATE [Pttcd].[Providers]
                SET [ProviderName] = @ProviderName,
                    [Alias] = @Alias,
                    [UkrlpProviderStatusDescription] = @ProviderStatus,
                    [UpdatedOn] = @UpdatedOn,
                    [UpdatedBy] = @UpdatedBy
                WHERE [ProviderId] = @ProviderId;";
            
            var updateContactDetailSql = $@"
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
                WHERE [ProviderId] = @ProviderId  AND  ContactType = 'P';";

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
                _logger.LogInformation("Updating [Pttcd].[Providers] table data for provider '{0}'", query.ProviderId.ToString());
                await transaction.Connection.ExecuteAsync(updateProviderSql, paramz, transaction);
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
                    _logger.LogInformation("Updating [Pttcd].[ProviderContacts] table data for provider '{0}'", query.ProviderId.ToString());
                    await transaction.Connection.ExecuteAsync(updateContactDetailSql, paramzContacts, transaction);
                }
            }

            return new Success();
        }
    }
}
