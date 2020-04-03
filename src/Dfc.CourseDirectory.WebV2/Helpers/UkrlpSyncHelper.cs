using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.Helpers.Interfaces;
using Dfc.CourseDirectory.WebV2.Security;
using Dfc.CourseDirectory.WebV2.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UkrlpService;

namespace Dfc.CourseDirectory.WebV2.Helpers
{
    public class UkrlpSyncHelper : IUkrlpSyncHelper
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

        public async Task SyncProviderData(Guid providerId, int ukprn, string updatedBy)
        {
            var providerData = this._ukrlpWcfService.GetProviderData(ukprn);

            if(providerData != null)
            {
                var upsertCommand = this.GetUpdateCommand(providerData);

                var existingProvider = await this.GetProvider(ukprn);

                if(existingProvider != null)
                {
                    upsertCommand.Id = existingProvider.Id;
                    upsertCommand.DateUpdated = _clock.UtcNow;
                    upsertCommand.UpdatedBy = updatedBy;
                    upsertCommand.Update = true;
                }
                else
                {
                    upsertCommand.Id = Guid.NewGuid();
                    upsertCommand.DateUpdated = _clock.UtcNow;
                    upsertCommand.UpdatedBy = updatedBy;
                    upsertCommand.UnitedKingdomProviderReferenceNumber = ukprn.ToString();
                    upsertCommand.ProviderType = Models.ProviderType.FE; //Set to FE by default
                    upsertCommand.Update = false;
                }

                await _cosmosDbQueryDispatcher.ExecuteQuery(upsertCommand);
            }
        }

        private UpsertProviderUkrlpData GetUpdateCommand(ProviderRecordStructure providerData)
        {
            var updateCommand = new UpsertProviderUkrlpData();

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
