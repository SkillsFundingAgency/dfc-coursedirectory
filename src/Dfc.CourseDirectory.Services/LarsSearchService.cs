using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Services.Interfaces;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<LarsSearchService> _logger;
        private readonly HttpClient _httpClient;
        private readonly Uri _uri;

        public LarsSearchService(
            ILogger<LarsSearchService> logger,
            HttpClient httpClient,
            IOptions<LarsSearchSettings> settings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(httpClient, nameof(httpClient));
            Throw.IfNull(settings, nameof(settings));

            _logger = logger;
            _httpClient = httpClient.Setup(settings.Value);
            _uri = settings.Value.ToUri();
        }



        public async Task<IResult<ILarsSearchResult>> SearchAsync(IZCodeSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));

            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Lars search criteria.", criteria);
                _logger.LogInformationObject("Lars search uri.", _uri);

                var content = new StringContent(criteria.ToJson(), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_uri, content);

                _logger.LogHttpResponseMessage("Lars search service http response.", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Lars search service json response.", json);

                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new LarsSearchResultContractResolver()
                    };

                    var searchResult = JsonConvert.DeserializeObject<LarsSearchResult>(json, settings);

                    return Result.Ok<ILarsSearchResult>(searchResult);
                }
                else
                {
                    return Result.Fail<ILarsSearchResult>("Lars search service unsuccessfull http response.");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Lars search service http request error.", hre);

                return Result.Fail<ILarsSearchResult>("Lars search service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Lars search service unknown error.", e);

                return Result.Fail<ILarsSearchResult>("Lars search service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }

        public async Task<IResult<ILarsSearchResult>> SearchAsync(ILarsSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));

            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Lars search criteria.", criteria);
                _logger.LogInformationObject("Lars search uri.", _uri);

                var content = new StringContent(criteria.ToJson(), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_uri, content);

                _logger.LogHttpResponseMessage("Lars search service http response.", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Lars search service json response.", json);

                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new LarsSearchResultContractResolver()
                    };

                    var searchResult = JsonConvert.DeserializeObject<LarsSearchResult>(json, settings);

                    return Result.Ok<ILarsSearchResult>(searchResult);
                }
                else
                {
                    return Result.Fail<ILarsSearchResult>("Lars search service unsuccessfull http response.");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Lars search service http request error.", hre);

                return Result.Fail<ILarsSearchResult>("Lars search service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Lars search service unknown error.", e);

                return Result.Fail<ILarsSearchResult>("Lars search service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
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
                ContractResolver = new LarsSearchCriteriaContractResolver()
            };

            settings.Converters.Add(new StringEnumConverter());

            var result = JsonConvert.SerializeObject(extendee, settings);

            return result;
        }

        internal static string ToJson(this IZCodeSearchCriteria extendee)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new LarsSearchCriteriaContractResolver()
            };

            settings.Converters.Add(new StringEnumConverter());

            var result = JsonConvert.SerializeObject(extendee, settings);

            return result;
        }
    }
}