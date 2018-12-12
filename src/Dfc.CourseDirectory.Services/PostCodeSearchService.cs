using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Dfc.CourseDirectory.Services.Tests")]

namespace Dfc.CourseDirectory.Services
{
    public class PostCodeSearchService : IPostCodeSearchService
    {
        private readonly ILogger<PostCodeSearchService> _logger;
        private readonly HttpClient _httpClient;
        private readonly Uri _uri;
        private readonly Uri _retrieveUri;
        private readonly String _APIKey;

        public PostCodeSearchService(
            ILogger<PostCodeSearchService> logger,
            HttpClient httpClient,
            IOptions<PostCodeSearchSettings> settings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(httpClient, nameof(httpClient));
            Throw.IfNull(settings, nameof(settings));

            _logger = logger;
            _httpClient = httpClient;
            _uri = settings.Value.ToUri();
            _retrieveUri = settings.Value.ToRetrieveUri();
            _APIKey = settings.Value.Key;
        }

        public async Task<IResult<IPostCodeSearchResult>> SearchAsync(IPostCodeSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));

            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("PostCode search criteria.", criteria);
                _logger.LogInformationObject("PostCode search uri.", _uri);

                var postCodeRequest = String.Concat(
                    _uri,

                     "&Key=" + System.Web.HttpUtility.UrlEncode(_APIKey),

                     "&SearchTerm='" + System.Web.HttpUtility.UrlEncode(criteria.Search.ToUpper()) + "'",
                     "&SearchFor=PostalCodes",
                     "&LastId=",
                     "&Country=GBR",
                     "&LanguagePreference=EN", 
                     "&MaxSuggestions=",
                     "&MaxResults="

                 );

                var response = await _httpClient.GetAsync(postCodeRequest);

                _logger.LogHttpResponseMessage("PostCode search service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new PostCodeSearchResultContractResolver()
                    };

                    var locations =
                        JsonConvert.DeserializeObject<IEnumerable<PostCodeSearchResultItem>>(json, settings);
                    var searchResult = new PostCodeSearchResult(locations)
                    {
                        Value = locations
                    };
                    return Result.Ok<IPostCodeSearchResult>(searchResult);
                }
                else
                {
                    return Result.Fail<IPostCodeSearchResult>("PostCode search service unsuccessful http response");
                }
            }

            catch (HttpRequestException hre)
            {
                _logger.LogException("PostCode search service http request error.", hre);

                return Result.Fail<IPostCodeSearchResult>("PostCode search service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("PostCode search service unknown error.", e);

                return Result.Fail<IPostCodeSearchResult>("PostCode search service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }


        public async Task<IResult<IAddressSelectionResult>> RetrieveAsync(IAddressSelectionCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));

            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Address selection criteria.", criteria);
                _logger.LogInformationObject("Address retrieve uri.", _uri);
                // string id = "GB|RM|B|51879423";
                //var RetrieveAddressBaseUrl = @"http://services.postcodeanywhere.co.uk/CapturePlus/Interactive/Retrieve/2.1/json.ws?";

                var addressRetrieveRequest = String.Concat(

                            _retrieveUri,

                            "&Key=" + System.Web.HttpUtility.UrlEncode(_APIKey),

                            "&Id=" + System.Web.HttpUtility.UrlEncode(criteria.Id)

                        );

                var response = await _httpClient.GetAsync(addressRetrieveRequest);

                _logger.LogHttpResponseMessage("Address retrieve service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new AddressSelectionResultContractResolver()
                    };

                    var address =
                        JsonConvert.DeserializeObject<IEnumerable<AddressSelectionResult>>(json, settings).ToList();

                    var searchResult = new AddressSelectionResult(address[0].Id, 
                        address[0].Line1, 
                        address[0].Line2,
                        address[0].City,
                        address[0].County,
                        address[0].PostCode);
                  
                    return Result.Ok<IAddressSelectionResult>(searchResult);
                }
                else
                {
                    return Result.Fail<IAddressSelectionResult>("PostCode search service unsuccessful http response");
                }
            }

            catch (HttpRequestException hre)
            {
                _logger.LogException("Address retrieve service http request error.", hre);

                return Result.Fail<IAddressSelectionResult>("Address retrieve service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Address retrieve service unknown error.", e);

                return Result.Fail<IAddressSelectionResult>("Address retrieve service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }
    }

    internal static class PostCodeSearchSettingsExtensions
    {
        internal static Uri ToUri(this IPostCodeSearchSettings extendee)
        {
            return new Uri($"{extendee.FindAddressesBaseUrl}");
        }

        internal static Uri ToRetrieveUri(this IPostCodeSearchSettings extendee)
        {
            return new Uri($"{extendee.RetrieveAddressBaseUrl}");
        }

        internal static String APIKey(this IPostCodeSearchSettings extendee)
        {
            return new String($"{extendee.Key}");
        }
    }

}