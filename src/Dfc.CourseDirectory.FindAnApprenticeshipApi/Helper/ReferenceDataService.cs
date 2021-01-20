using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Services;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Helper
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