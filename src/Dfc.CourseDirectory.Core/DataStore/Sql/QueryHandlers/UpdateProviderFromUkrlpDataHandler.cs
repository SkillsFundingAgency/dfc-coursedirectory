using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;
using System.Data.SqlClient;
using Dapper;
using Dfc.CourseDirectory.Core.ReferenceData.Ukrlp;
using Microsoft.Extensions.Logging;
using System;
using System.Data;

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

            var sqlProviderContact = "UPDATE Pttcd.ProviderContacts SET ContactType = @contactType,"
                + "AddressSaonDescription = @addressSaonDescription,"
                + "AddressPaonDescription = @addressPaonDescription,"
                + "AddressStreetDescription = @addressStreetDescription,"
                + "AddressLocality = @addressLocality,"
                + "AddressItems = @addressItems,"
                + "AddressPostTown = @addressPostTown,"
                + "AddressCounty = @addressCounty,"
                + "AddressPostcode = @addressPostcode,"
                + "PersonalDetailsPersonNameTitle = @personalDetailsPersonNameTitle,"
                + "PersonalDetailsPersonNameGivenName = @personalDetailsPersonNameGivenName,"
                + "PersonalDetailsPersonNameFamilyName = @personalDetailsPersonNameFamilyName,"
                + "Telephone1 = @telephone1,"
                + "Fax = @fax,"
                + "WebsiteAddress = @websiteAddress,"
                + "Email = @email,"
                + "WHERE ProviderId = @providerId AND ContactType = 'P';";

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
                _logger.LogInformation(" *** SET ***");
                _logger.LogInformation("ContactType: {0}", providerContact.ContactType);
                _logger.LogInformation("AddressSaonDescription: {0}", providerContact.AddressSaonDescription);
                _logger.LogInformation("AddressPaonDescription: {0}", providerContact.AddressPaonDescription);
                _logger.LogInformation("AddressStreetDescription: {0}", providerContact.AddressStreetDescription);
                _logger.LogInformation("AddressLocality: {0}", providerContact.AddressLocality);
                _logger.LogInformation("AddressItems: {0}", providerContact.AddressItems);
                _logger.LogInformation("AddressPostTown: {0}", providerContact.AddressPostTown);
                _logger.LogInformation("AddressCounty: {0}", providerContact.AddressCounty);
                _logger.LogInformation("AddressPostcode: {0}", providerContact.AddressPostcode);
                _logger.LogInformation("PersonalDetailsPersonNameTitle: {0}", providerContact.PersonalDetailsPersonNameTitle);
                _logger.LogInformation("PersonalDetailsPersonNameGivenName: {0}", providerContact.PersonalDetailsPersonNameGivenName);
                _logger.LogInformation("PersonalDetailsPersonNameFamilyName: {0}", providerContact.PersonalDetailsPersonNameFamilyName);
                _logger.LogInformation("Telephone1: {0}", providerContact.Telephone1);
                _logger.LogInformation("Fax: {0}", providerContact.Fax);
                _logger.LogInformation("WebsiteAddress: {0}", providerContact.WebsiteAddress);
                _logger.LogInformation("Email: {0}", providerContact.Email);
                _logger.LogInformation("ProviderId: {0}", query.ProviderId);

                if (query.UpdateProviderContact)
                {
                    using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("DefaultConnection")))
                    {
                        SqlCommand command = new SqlCommand(sqlProviderContact, connection);
                        command.Parameters.Add("@providerId", SqlDbType.UniqueIdentifier);
                        command.Parameters["@providerId"].Value = query.ProviderId;

                        command.Parameters.AddWithValue("@contactType", providerContact.ContactType);
                        command.Parameters.AddWithValue("@addressSaonDescription", providerContact.AddressSaonDescription);
                        command.Parameters.AddWithValue("@addressPaonDescription", providerContact.AddressPaonDescription);
                        command.Parameters.AddWithValue("@addressStreetDescription", providerContact.AddressStreetDescription);
                        command.Parameters.AddWithValue("@addressLocality", providerContact.AddressLocality);
                        command.Parameters.AddWithValue("@addressItems", providerContact.AddressItems);
                        command.Parameters.AddWithValue("@addressPostTown", providerContact.AddressPostTown);
                        command.Parameters.AddWithValue("@addressCounty", providerContact.AddressCounty);
                        command.Parameters.AddWithValue("@addressPostcode", providerContact.AddressPostcode);
                        command.Parameters.AddWithValue("@personalDetailsPersonNameTitle", providerContact.PersonalDetailsPersonNameTitle);
                        command.Parameters.AddWithValue("@personalDetailsPersonNameGivenName", providerContact.PersonalDetailsPersonNameGivenName);
                        command.Parameters.AddWithValue("@personalDetailsPersonNameFamilyName", providerContact.PersonalDetailsPersonNameFamilyName);
                        command.Parameters.AddWithValue("@telephone1", providerContact.Telephone1);
                        command.Parameters.AddWithValue("@fax", providerContact.Fax);
                        command.Parameters.AddWithValue("@websiteAddress", providerContact.WebsiteAddress);
                        command.Parameters.AddWithValue("@email", providerContact.Email);

                        _logger.LogInformation("Update [Pttcd].[ProviderContacts] starting...");

                        try
                        {
                            connection.Open();
                            int rowsAffected = await command.ExecuteNonQueryAsync();

                            _logger.LogInformation("Update [Pttcd].[ProviderContacts] finished!");
                            _logger.LogInformation("Impacted rows: {0}", rowsAffected);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("ERROR! {0}", ex);
                            throw new NotImplementedException();
                        }
                    }
                }
            }
            
            return new Success();
        }
    }
}
