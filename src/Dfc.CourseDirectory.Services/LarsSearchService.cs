using Dfc.CourseDirectory.Services.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Dfc.CourseDirectory.Services.Tests")]

namespace Dfc.CourseDirectory.Services
{
    public class LarsSearchService : ILarsSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _uri;

        public LarsSearchService(HttpClient httpClient, IOptions<LarsSearchSettings> settings)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _httpClient = _httpClient.Setup(settings.Value);
            _uri = settings.Value.ToUri();
        }

        public ILarsSearchResult Search(ILarsSearchCriteria criteria)
        {
            throw new NotImplementedException();
        }

        public async Task<ILarsSearchResult> SearchAsync(ILarsSearchCriteria criteria)
        {
            try
            {
                var content = new StringContent(criteria.ToJson(), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_uri, content);
                var json = await response.Content.ReadAsStringAsync();
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new AtDotcaseContractResolver()
                };

                var result = JsonConvert.DeserializeObject<LarsSearchResult>(json, settings);

                return result;
            }
            catch (Exception e)
            {
                // TODO: log the exception
                throw e;
            }

            throw new NotImplementedException();
        }
    }

    internal static class HttpClientExtensions
    {
        internal static HttpClient Setup(this HttpClient extendee, ILarsSearchSettings settings)
        {
            extendee.DefaultRequestHeaders.Add("api-key", settings.ApiKey);
            extendee.DefaultRequestHeaders.Add("api-version", settings.ApiVersion);
            extendee.DefaultRequestHeaders.Add("indexes", settings.Indexes);

            return extendee;
        }
    }

    internal static class LarsSearchSettingsExtensions
    {
        internal static Uri ToUri(this ILarsSearchSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl}?api-version={extendee.ApiVersion}");
        }
    }

    internal static class LarsSearchCriteriaExtensions
    {
        internal static string ToJson(this ILarsSearchCriteria extendee)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new LowercaseContractResolver()
            };

            settings.Converters.Add(new StringEnumConverter() { CamelCaseText = false });

            var result = JsonConvert.SerializeObject(extendee, settings);

            return result;
        }
    }
}
