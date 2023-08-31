using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.SharedViews.Components;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features
{
    public class ProviderInfoPanelViewComponent : ViewComponent
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public ProviderInfoPanelViewComponent(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid providerId)
        {
            var provider = await _sqlQueryDispatcher.ExecuteQuery(
                new GetProviderById()
                {
                    ProviderId = providerId
                });

            if (provider == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Provider, providerId);
            }
            var providerContact = await _sqlQueryDispatcher.ExecuteQuery(new GetProviderContactById { ProviderId = providerId });


         
            ProviderInfoPanelViewModel vm;

            if (providerContact == null)
            {
                vm = new ProviderInfoPanelViewModel()
                {
                    ProviderId = providerId,
                    GotContact = false
                };
            }
            else
            {
                var contactName = providerContact.PersonalDetailsPersonNameGivenName != null && providerContact.PersonalDetailsPersonNameFamilyName != null ?
                    $"{string.Join(" ", providerContact.PersonalDetailsPersonNameGivenName)} {providerContact.PersonalDetailsPersonNameFamilyName}" :
                    null;

                vm = new ProviderInfoPanelViewModel()
                {
                    AddressParts = FormatAddress(providerContact),
                    ContactName = contactName,
                    Email = providerContact.Email,
                    GotContact = true,
                    ProviderId = provider.ProviderId,
                    Telephone = providerContact.Telephone1,
                    Website = providerContact.WebsiteAddress != null ? UrlUtil.EnsureHttpPrefixed(providerContact.WebsiteAddress) : null
                };
            }

            return View("~/SharedViews/Components/ProviderInfoPanel.cshtml", vm);

            static IReadOnlyCollection<string> FormatAddress(ProviderContact address)
            {
                var parts = new List<string>
                {
                    address.AddressSaonDescription,
                    address.AddressPaonDescription,
                    address.AddressStreetDescription,
                    address.AddressLocality,
                    address.AddressPostcode
                };

                return parts.Where(p => !string.IsNullOrEmpty(p)).ToList();
            }
        }
    }
}
