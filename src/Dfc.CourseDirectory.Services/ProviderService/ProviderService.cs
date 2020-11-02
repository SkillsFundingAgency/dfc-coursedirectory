using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Models.Interfaces.Providers;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
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

            _getProviderByPRNUri = ToGetProviderByPRNUri(settings.Value);
            _updateProviderByIdUri = ToUpdateProviderByIdUri(settings.Value);
            _updateProviderDetailsUri = ToUpdateProviderDetailsUri(settings.Value);
        }

        public async Task<IResult<IEnumerable<Provider>>> GetProviderByPRNAsync(string prn)
        {
            if (string.IsNullOrWhiteSpace(prn))
            {
                throw new ArgumentException($"{prn} cannot be null or empty or whitespace.", nameof(prn));
            }

            try
            {
                _logger.LogInformationObject("Provider search criteria.", prn);
                _logger.LogInformationObject("Provider search URI", _getProviderByPRNUri);

                // dependency injection not working for _httpClient when this is called from async code so use our own local version
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await httpClient.GetAsync($"{_getProviderByPRNUri}?PRN={prn}");

                _logger.LogHttpResponseMessage("Provider search service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!json.StartsWith("["))
                        json = "[" + json + "]";

                    _logger.LogInformationObject("Provider search service json response", json);

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
                _logger.LogException("Provider search service http request error", hre);
                return Result.Fail<IEnumerable<Provider>>("Provider search service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Provider search service unknown error.", e);

                return Result.Fail<IEnumerable<Provider>>("Provider search service unknown error.");
            }
        }

        public async Task<IResult<IProvider>> AddProviderAsync(IProviderAdd provider)
        {
            Throw.IfNull(provider, nameof(provider));

            try
            {
                _logger.LogInformationObject("Provider add object.", provider);
                _logger.LogInformationObject("Provider add URI", _updateProviderByIdUri);

                var providerJson = JsonConvert.SerializeObject(provider);

                var content = new StringContent(providerJson, Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.PostAsync(_updateProviderByIdUri, content);

                _logger.LogHttpResponseMessage("Provider add service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Provider add service json response", json);


                    var providerResult = JsonConvert.DeserializeObject<Provider>(json);


                    return Result.Ok<IProvider>(providerResult);
                }
                else
                {
                    return Result.Fail<IProvider>("Provider add service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Provider add service http request error", hre);
                return Result.Fail<IProvider>("Provider add service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Provider add service unknown error.", e);

                return Result.Fail<IProvider>("Provider add service unknown error.");
            }
        }

        public async Task<IResult> UpdateProviderDetails(IProvider provider)
        {
            Throw.IfNull(provider, nameof(provider));

            try
            {
                _logger.LogInformationObject("Provider add object.", provider);
                _logger.LogInformationObject("Provider add URI", _updateProviderDetailsUri);

                var providerJson = JsonConvert.SerializeObject(provider);

                var content = new StringContent(providerJson, Encoding.UTF8, "application/json");
                // threading vs DI issues
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await httpClient.PostAsync(_updateProviderDetailsUri, content);

                _logger.LogHttpResponseMessage("Provider update service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    // NOTE: There is no response content payload returned from this api - why? - don't know.
                    // Therefore commenting this bit out as it will/does cause an exception down stream.
                    //var json = await response.Content.ReadAsStringAsync();

                    //_logger.LogInformationObject("Provider update service json response", json);

                    // NOTE: deserialising the "providerJson" var set earlier to allow code down stream to run.
                    var providerResult = JsonConvert.DeserializeObject<Provider>(providerJson);


                    return Result.Ok(providerResult);
                }
                else
                {
                    return Result.Fail("Provider update service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Provider update service http request error", hre);
                return Result.Fail("Provider adupdated service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Provider update service unknown error.", e);

                return Result.Fail("Provider update service unknown error.");
            }
        }

        private static Uri ToGetProviderByPRNUri(IProviderServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetProviderByPRN");
        }

        internal static Uri ToUpdateProviderByIdUri(IProviderServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/UpdateProviderById");
        }

        internal static Uri ToUpdateProviderDetailsUri(IProviderServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/UpdateProviderDetails");
        }
    }
}
