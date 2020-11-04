using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.Services.ProviderService
{
    public class ProviderService : IProviderService
    {
        private readonly ILogger<ProviderService> _logger;
        private readonly ProviderServiceSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly Uri _getProviderByPRNUri;
        private readonly Uri _updateProviderByIdUri;
        private readonly Uri _updateProviderDetailsUri;

        public ProviderService(
            ILogger<ProviderService> logger,
            HttpClient httpClient,
            IOptions<ProviderServiceSettings> settings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(httpClient, nameof(httpClient));
            Throw.IfNull(settings, nameof(settings));

            _logger = logger;
            _settings = settings.Value;
            _httpClient = httpClient;

            _getProviderByPRNUri = settings.Value.ToGetProviderByPRNUri();
            _updateProviderByIdUri = settings.Value.ToUpdateProviderByIdUri();
            _updateProviderDetailsUri = settings.Value.ToUpdateProviderDetailsUri();
        }

        public async Task<Result<IEnumerable<Provider>>> GetProviderByPRNAsync(string prn)
        {
            if (string.IsNullOrWhiteSpace(prn))
            {
                throw new ArgumentException($"{prn} cannot be null or empty or whitespace.", nameof(prn));
            }

            try
            {
                // dependency injection not working for _httpClient when this is called from async code so use our own local version
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await httpClient.GetAsync($"{_getProviderByPRNUri}?PRN={prn}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!json.StartsWith("["))
                        json = "[" + json + "]";

                    var providers = JsonConvert.DeserializeObject<IEnumerable<Provider>>(json);

                    return Result.Ok(providers);
                }
                else
                {
                    return Result.Fail<IEnumerable<Provider>>("Provider search service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Provider search service http request error");
                return Result.Fail<IEnumerable<Provider>>("Provider search service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Provider search service unknown error.");
                return Result.Fail<IEnumerable<Provider>>("Provider search service unknown error.");
            }
        }

        public async Task<Result<Provider>> AddProviderAsync(ProviderAdd provider)
        {
            Throw.IfNull(provider, nameof(provider));

            try
            {
                var providerJson = JsonConvert.SerializeObject(provider);

                var content = new StringContent(providerJson, Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.PostAsync(_updateProviderByIdUri, content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var providerResult = JsonConvert.DeserializeObject<Provider>(json);

                    return Result.Ok(providerResult);
                }
                else
                {
                    return Result.Fail<Provider>("Provider add service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Provider add service http request error");
                return Result.Fail<Provider>("Provider add service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Provider add service unknown error.");
                return Result.Fail<Provider>("Provider add service unknown error.");
            }
        }

        public async Task<Result> UpdateProviderDetails(Provider provider)
        {
            Throw.IfNull(provider, nameof(provider));

            try
            {
                var providerJson = JsonConvert.SerializeObject(provider);

                var content = new StringContent(providerJson, Encoding.UTF8, "application/json");

                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await httpClient.PostAsync(_updateProviderDetailsUri, content);

                if (response.IsSuccessStatusCode)
                {
                    var providerResult = JsonConvert.DeserializeObject<Provider>(providerJson);

                    return Result.Ok();
                }
                else
                {
                    return Result.Fail("Provider update service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Provider update service http request error");
                return Result.Fail("Provider adupdated service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Provider update service unknown error.");
                return Result.Fail("Provider update service unknown error.");
            }
        }
    }
}
