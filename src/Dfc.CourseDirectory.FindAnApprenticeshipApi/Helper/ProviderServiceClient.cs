using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Helper;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Services;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models.Providers;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Helper
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