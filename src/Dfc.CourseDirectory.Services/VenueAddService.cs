using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Services
{
    public class VenueAddService : IVenueAddService
    {
        private readonly ILogger<VenueAddService> _logger;
        private readonly HttpClient _httpClient;
        private readonly Uri _uri;

        public VenueAddService(
            ILogger<VenueAddService> logger,
            HttpClient httpClient,
            IOptions<VenueAddSettings> settings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(httpClient, nameof(httpClient));
            Throw.IfNull(settings, nameof(settings));

            _logger = logger;
            _httpClient = httpClient;
            
            //_uri = settings.Value.ToUri();
        }
        public async Task<IResult<IVenueAddResultItem>> AddAsync(IVenueAdd venue)
        {
            Throw.IfNull(venue, nameof(venue));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Venue add object.", venue);
                _logger.LogInformationObject("Venue search URI", _uri);

                var venueJson = JsonConvert.SerializeObject(venue);

                var content = new StringContent(venueJson, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_uri, content);

                _logger.LogHttpResponseMessage("Venue add service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Venue add service json response", json);

                    //var settings = new JsonSerializerSettings
                    //{
                    //    ContractResolver = new VenueSearchResultContractResolver()
                    //};
                    var venueResult = JsonConvert.DeserializeObject<VenueAddResultItem>(json);

                   //VenueAddResultItem item = new VenueAddResultItem("","","","","","","","");

                    //var searchResult = new VenueSearchResult(venues)
                    //{
                    //    Value = venues.OrderBy(x => x.VenueName)
                    //};


                    return Result.Ok<IVenueAddResultItem>(venueResult);
                }
                else
                {
                    return Result.Fail<IVenueAddResultItem>("Venue add service unsuccessful http response");
                }
            }

            catch (HttpRequestException hre)
            {
                _logger.LogException("Venue add service http request error", hre);
                return Result.Fail<IVenueAddResultItem>("Venue add service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Venue add service unknown error.", e);

                return Result.Fail<IVenueAddResultItem>("Venue add service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
            
        }

    }
    internal static class VenueAddSettingsExtensions
    {
        internal static Uri ToUri(this IVenueAddSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "/addvenue?code=" + extendee.ApiKey}");
            //return new Uri($"{extendee.ApiUrl}?api-version={extendee.ApiVersion}");
        }
    }
    //internal static class VenueAddExtensions
    //{
    //    internal static string ToJson(this IVenueAdd extendee)
    //    {

    //        VenueSearchJson json = new VenueSearchJson
    //        {
    //            PRN = extendee.Search
    //        };
    //        var result = JsonConvert.SerializeObject(json);

    //        return result;
    //    }
    //}

    //internal class VenueSearchJson
    //{
    //    public string PRN { get; set; }
    //}

}
