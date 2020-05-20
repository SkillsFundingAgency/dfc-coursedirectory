using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.ReferenceData.Ukrlp;
using UkrlpService;

namespace Dfc.CourseDirectory.WebV2.Helpers
{
    public class UkrlpSyncHelper
    {
        private readonly IUkrlpWcfService _ukrlpWcfService;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IClock _clock;

        public UkrlpSyncHelper(IUkrlpWcfService ukrlpWcfService, 
                                ICosmosDbQueryDispatcher cosmosDbQueryDispatcher, 
                                IClock clock)
        {
            _ukrlpWcfService = ukrlpWcfService;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _clock = clock;
        }

        public async Task SyncProviderData(int ukprn, string updatedBy)
        {
            var providerData = await _ukrlpWcfService.GetProviderData(ukprn);

            if(providerData != null)
            {
                var upsertCommand = GetUpdateCommand(providerData);

                var existingProvider = await GetProvider(ukprn);

                // Update
                if (existingProvider != null)
                {
                    upsertCommand.Id = existingProvider.Id;
                    upsertCommand.DateUpdated = _clock.UtcNow;
                    upsertCommand.UpdatedBy = updatedBy;

                    await _cosmosDbQueryDispatcher.ExecuteQuery(upsertCommand);
                }
                // Insert
                else
                {
                    var insertCommand = new CreateProviderFromUkrlpData()
                    {
                        Id = Guid.NewGuid(),
                        DateUpdated = _clock.UtcNow,
                        UpdatedBy = updatedBy,
                        UnitedKingdomProviderReferenceNumber = ukprn.ToString(),
                        ProviderType = ProviderType.Both, 
                        ProviderContact = upsertCommand.ProviderContact,
                        ProviderName = upsertCommand.ProviderName,
                        ProviderStatus = upsertCommand.ProviderStatus,
                    };

                    await _cosmosDbQueryDispatcher.ExecuteQuery(insertCommand);
                }

            }
        }

        private UpdateProviderFromUkrlpData GetUpdateCommand(ProviderRecordStructure providerData)
        {
            var updateCommand = new UpdateProviderFromUkrlpData();

            // Build contacts
            List<ProviderContact> providercontacts = new List<ProviderContact>();
            var ukrlpDataContacts = providerData.ProviderContact
                                                .Where(p => p.ContactType == "P")
                                                .OrderByDescending(c => c.LastUpdated);

            // Load UKRLP api contacts if available
            if (ukrlpDataContacts.Any())
            {
                var ukrlpContact = ukrlpDataContacts.First();

                // Build contact address
                ContactAddress contactaddress = new ContactAddress()
                {
                    SAON = new SAON() { Description = ukrlpContact.ContactAddress.SAON.Description },
                    PAON = new PAON() { Description = ukrlpContact.ContactAddress.PAON.Description },
                    StreetDescription = ukrlpContact.ContactAddress.StreetDescription,
                    Locality = ukrlpContact.ContactAddress.Locality,
                    Items = ukrlpContact.ContactAddress.Items,
                    PostCode = ukrlpContact.ContactAddress.PostCode,
                };

                // Build contact personal details
                ContactPersonalDetails contactpersonaldetails = new ContactPersonalDetails()
                {
                    PersonNameTitle = ukrlpContact.ContactPersonalDetails.PersonNameTitle,
                    PersonGivenName = ukrlpContact.ContactPersonalDetails.PersonGivenName,
                    PersonFamilyName = ukrlpContact.ContactPersonalDetails.PersonFamilyName,
                };

                var providerContact = new ProviderContact();
                providerContact.ContactType = ukrlpContact.ContactType;
                providerContact.ContactTelephone1 = ukrlpContact.ContactTelephone1;
                providerContact.ContactWebsiteAddress = ukrlpContact.ContactWebsiteAddress;
                providerContact.ContactEmail = ukrlpContact.ContactEmail;
                providerContact.LastUpdated = ukrlpContact.LastUpdated;
                providerContact.ContactAddress = contactaddress;
                providerContact.ContactPersonalDetails = contactpersonaldetails;

                providercontacts.Add(providerContact);

                //Add to provider
                updateCommand.ProviderContact = providercontacts;
            }

            updateCommand.ProviderName = providerData.ProviderName;
            updateCommand.ProviderStatus = providerData.ProviderStatus;
            updateCommand.Alias = providerData.ProviderAliases?.FirstOrDefault()?.ProviderAlias;

            return updateCommand;
        }

        private Task<Provider> GetProvider(int ukprn) =>
           _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderByUkprn() { Ukprn = ukprn });
    }
}
