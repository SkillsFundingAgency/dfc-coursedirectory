using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Services
{
    public class ProviderSearchService : IProviderSearchService
    {
        private readonly ILogger<ProviderSearchService> _logger;
        private readonly HttpClient _httpClient;
        private readonly Uri _uri;

        public ProviderSearchService(
            ILogger<ProviderSearchService> logger,
            HttpClient httpClient,
            IOptions<ProviderSearchSettings> settings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(httpClient, nameof(httpClient));
            Throw.IfNull(settings, nameof(settings));

            _logger = logger;
            _httpClient = httpClient;

            _uri = settings.Value.ToUri();
        }

        public async Task<IResult<IProviderSearchResult>> SearchAsync(IProviderSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Provider search criteria.", criteria);
                _logger.LogInformationObject("Provider search URI", _uri);

                var content = new StringContent(criteria.ToJson(), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_uri, content);

                _logger.LogHttpResponseMessage("Provider search service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!json.StartsWith("["))
                        json = "[" + json + "]";

                    _logger.LogInformationObject("Provider search service json response", json);

                    var providers = JsonConvert.DeserializeObject<IEnumerable<Provider>>(json);

                    var searchResult = new ProviderSearchResult(providers)
                    {
                        Value = providers
                    };

                    return Result.Ok<IProviderSearchResult>(searchResult);
                }
                else
                {
                    return Result.Fail<IProviderSearchResult>("Provider search service unsuccessful http response");
                }
            }

            catch (HttpRequestException hre)
            {
                _logger.LogException("Provider search service http request error", hre);
                return Result.Fail<IProviderSearchResult>("Provider search service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Provider search service unknown error.", e);

                return Result.Fail<IProviderSearchResult>("Provider search service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }

        }
    }

    internal static class ProviderSearchSettingsExtensions
    {
        internal static Uri ToUri(this IProviderSearchSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + extendee.ApiKey}");
            //return new Uri($"{extendee.ApiUrl}?api-version={extendee.ApiVersion}");
        }
    }
    internal static class ProviderSearchCriteriaExtensions
    {
        internal static string ToJson(this IProviderSearchCriteria extendee)
        {

            ProviderSearchJson json = new ProviderSearchJson
            {
                PRN = extendee.Search
            };
            var result = JsonConvert.SerializeObject(json);

            return result;
        }
    }

    internal class ProviderSearchJson
    {
        public string PRN { get; set; }
    }
}
