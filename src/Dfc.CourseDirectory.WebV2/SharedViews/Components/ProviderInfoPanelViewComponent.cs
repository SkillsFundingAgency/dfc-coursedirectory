using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.SharedViews.Components;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features
{
    public class ProviderInfoPanelViewComponent : ViewComponent
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public ProviderInfoPanelViewComponent(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid providerId)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetProviderById()
                {
                    ProviderId = providerId
                });

            if (provider == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Provider, providerId);
            }

            var contact = provider.ProviderContact
                .OrderByDescending(c => c.LastUpdated)
                .SingleOrDefault(c => c.ContactType == "P");  // 'P' == Primary

            ProviderInfoPanelViewModel vm;

            if (contact == null)
            {
                vm = new ProviderInfoPanelViewModel()
                {
                    ProviderId = providerId,
                    GotContact = false
                };
            }
            else
            {
                var contactName = contact.ContactPersonalDetails?.PersonGivenName != null && contact.ContactPersonalDetails?.PersonFamilyName != null ?
                    $"{string.Join(" ", contact.ContactPersonalDetails.PersonGivenName)} {contact.ContactPersonalDetails.PersonFamilyName}" :
                    null;

                vm = new ProviderInfoPanelViewModel()
                {
                    AddressParts = contact.ContactAddress != null ?
                        FormatAddress(contact.ContactAddress) :
                        Array.Empty<string>(),
                    ContactName = contactName,
                    Email = contact.ContactEmail,
                    GotContact = true,
                    ProviderId = provider.Id,
                    Telephone = contact.ContactTelephone1,
                    Website = contact.ContactWebsiteAddress != null ? UrlUtil.EnsureHttpPrefixed(contact.ContactWebsiteAddress) : null
                };
            }

            return View("~/SharedViews/Components/ProviderInfoPanel.cshtml", vm);

            static IReadOnlyCollection<string> FormatAddress(ProviderContactAddress address)
            {
                var parts = new List<string>()
                {
                    address.SAON?.Description,
                    address.PAON?.Description,
                    address.StreetDescription,
                    address.Locality
                };

                if (address.Items != null)
                {
                    parts.AddRange(address.Items);
                }

                parts.Add(address.PostCode);

                return parts.Where(p => !string.IsNullOrEmpty(p)).ToList();
            }
        }
    }
}
