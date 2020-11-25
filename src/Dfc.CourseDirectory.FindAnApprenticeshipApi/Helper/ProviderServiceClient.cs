using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.Providerportal.FindAnApprenticeship.Interfaces.Helper;
using Dfc.Providerportal.FindAnApprenticeship.Interfaces.Services;
using Dfc.Providerportal.FindAnApprenticeship.Models.Providers;

namespace Dfc.Providerportal.FindAnApprenticeship.Helper
{
    public class ProviderServiceClient : IProviderServiceClient
    {
        private readonly IProviderService _service;

        public ProviderServiceClient(IProviderService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public async Task<IEnumerable<Provider>> GetAllProviders()
        {
            try
            {
                return await _service.GetActiveProvidersAsync();
            }
            catch (Exception e)
            {
                throw new ProviderServiceException(e);
            }
        }
    }
}