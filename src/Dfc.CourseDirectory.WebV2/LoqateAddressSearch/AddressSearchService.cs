using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.WebV2.LoqateAddressSearch
{
    public class AddressSearchService : IAddressSearchService
    {
        private const string FindByPostcodeBaseUrl = "https://services.postcodeanywhere.co.uk/PostcodeAnywhere/Interactive/FindByPostcode/v1.00/json3.ws";
        private const string RetrieveByIdBaseUrl = "https://services.postcodeanywhere.co.uk/PostcodeAnywhere/Interactive/RetrieveById/v1.30/json3.ws";

        private readonly HttpClient _httpClient;
        private readonly Options _options;

        public AddressSearchService(HttpClient httpClient, Options options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        public async Task<AddressDetail> GetById(string id)
        {
            var url = new Url(RetrieveByIdBaseUrl)
                .SetQueryParam("Key", _options.Key)
                .SetQueryParam("Id", id);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            try
            {
                return ExtractItems<AddressDetail>(responseJson).SingleOrDefault();
            }
            catch (LoqateErrorException ex) when (ex.Error == "1002")
            {
                return null;
            }
        }

        public async Task<IReadOnlyCollection<PostcodeSearchResult>> SearchByPostcode(string postcode)
        {
            var url = new Url(FindByPostcodeBaseUrl)
                .SetQueryParam("Key", _options.Key)
                .SetQueryParam("Postcode", postcode)
                .SetQueryParam("SearchFor", "PostalCodes");

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            return ExtractItems<PostcodeSearchResult>(responseJson) ?? Array.Empty<PostcodeSearchResult>();
        }

        private static IReadOnlyCollection<T> ExtractItems<T>(string json)
        {
            var response = JsonConvert.DeserializeObject<Response>(json);

            if (response.Items.Count == 0)
            {
                return default;
            }

            if (response.Items[0].Property("Error") != null)
            {
                var error = response.Items[0].ToObject<ErrorItem>();

                throw new LoqateErrorException(error.Error, error.Description, error.Cause, error.Resolution);
            }

            return response.Items.Select(i => i.ToObject<T>()).ToList();
        }

        private class Response
        {
            public IReadOnlyList<JObject> Items { get; set; }
        }

        private class ErrorItem
        {
            public string Error { get; set; }
            public string Description { get; set; }
            public string Cause { get; set; }
            public string Resolution { get; set; }
        }
    }
}
