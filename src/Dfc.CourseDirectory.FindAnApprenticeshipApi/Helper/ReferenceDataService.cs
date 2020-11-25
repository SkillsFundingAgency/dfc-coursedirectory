using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.Providerportal.FindAnApprenticeship.Interfaces.Services;
using Dfc.Providerportal.FindAnApprenticeship.Models;
using Newtonsoft.Json;

namespace Dfc.Providerportal.FindAnApprenticeship.Helper
{
    public class ReferenceDataService : IReferenceDataService
    {
        private readonly HttpClient _httpClient;

        public ReferenceDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<FeChoice>> GetAllFeChoicesAsync()
        {
            // TODO: Request config changes from devops to remove 'FeChoices' from the base URL
            var response = await _httpClient.GetAsync("");

            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<IEnumerable<FeChoice>>(await response.Content.ReadAsStringAsync());
        }
    }
}