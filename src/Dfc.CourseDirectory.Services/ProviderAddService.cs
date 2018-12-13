using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Interfaces.Providers;
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
    public class ProviderAddService : IProviderAddService
    {
        private readonly ILogger<ProviderAddService> _logger;
        private readonly HttpClient _httpClient;
        private readonly Uri _uri;

        public ProviderAddService(
            ILogger<ProviderAddService> logger,
            HttpClient httpClient,
            IOptions<ProviderAddSettings> settings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(httpClient, nameof(httpClient));
            Throw.IfNull(settings, nameof(settings));

            _logger = logger;
            _httpClient = httpClient;

            _uri = settings.Value.ToUri();

        }

        public async Task<IResult<IProvider>> AddProviderAsync(IProviderAdd provider)
        {
            Throw.IfNull(provider, nameof(provider));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Provider add object.", provider);
                _logger.LogInformationObject("Provider add URI", _uri);

                var providerJson = JsonConvert.SerializeObject(provider);

                var content = new StringContent(providerJson, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_uri, content);

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
            finally
            {
                _logger.LogMethodExit();
            }
        }
    }

    internal static class ProviderAddSettingsExtensions
    {
        internal static Uri ToUri(this IProviderAddSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl}{extendee.ApiKey}");
        }
    }
}
