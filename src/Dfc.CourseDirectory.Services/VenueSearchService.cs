using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Services
{
    public class VenueSearchService : IVenueSearchService
    {
        private readonly ILogger<VenueSearchService> _logger;
        private readonly HttpClient _httpClient;
        private readonly Uri _uri;

        public VenueSearchService(
            ILogger<VenueSearchService> logger,
            HttpClient httpClient,
            IOptions<VenueSearchSettings> settings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(httpClient, nameof(httpClient));
            Throw.IfNull(settings, nameof(settings));

            _logger = logger;
            _httpClient = httpClient;
            
            _uri = settings.Value.ToUri();
        }
        public async Task<IResult<IVenueSearchResult>> SearchAsync(IVenueSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Venue search criteria.", criteria);
                _logger.LogInformationObject("Venue search URI", _uri);


                //var toJson = criteria.ToJson();
                var content = new StringContent(criteria.ToJson(), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_uri, content);

                _logger.LogHttpResponseMessage("Venue search service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    
                    _logger.LogInformationObject("Venue search service json response", json);

                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new VenueSearchResultContractResolver()
                    };
                    var venues  = JsonConvert.DeserializeObject<IEnumerable<VenueSearchResultItem>>(json, settings);
                    var searchResult = new VenueSearchResult(venues)
                    {
                        Value = venues
                    };
                    //searchResult.Value = venues;
                    return Result.Ok<IVenueSearchResult>(searchResult);
                }
                else
                {
                    return Result.Fail<IVenueSearchResult>("Venue search service unsuccessful http response");
                }
            }

            catch (HttpRequestException hre)
            {
                _logger.LogException("Venue search service http request error", hre);
                return Result.Fail<IVenueSearchResult>("Venue search service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Venue search service unknown error.", e);

                return Result.Fail<IVenueSearchResult>("Venue search service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
            
        }

    }
    internal static class VenueSearchSettingsExtensions
    {
        internal static Uri ToUri(this IVenueSearchSettings extendee)
        {
            //https://dfc-dev-prov-venue-fa.azurewebsites.net/api/GetVenuesByPRN?code=V0NA6ETyLGHZelFrpJaXcLyVUWxM5OqSOZGHHXS7an5R2FCt84QI1Q==
            return new Uri($"{extendee.ApiUrl + extendee.ApiKey}");
            //return new Uri($"{extendee.ApiUrl}?api-version={extendee.ApiVersion}");
        }
    }
    internal static class VenueSearchCriteriaExtensions
    {
        internal static string ToJson(this IVenueSearchCriteria extendee)
        {

            VenueSearchJson json = new VenueSearchJson
            {
                PRN = extendee.Search
            };
            var result = JsonConvert.SerializeObject(json);

            return result;
        }
    }

    internal class VenueSearchJson
    {
        public string PRN { get; set; }
    }

}
