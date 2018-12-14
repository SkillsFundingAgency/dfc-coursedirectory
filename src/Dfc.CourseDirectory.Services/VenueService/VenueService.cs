using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Models.Venues;
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
using Dfc.CourseDirectory.Services.Interfaces.VenueService;

namespace Dfc.CourseDirectory.Services.VenueService
{
    public class VenueService : IVenueService
    {
        private readonly ILogger<VenueService> _logger;
        private readonly HttpClient _httpClient;
        private readonly Uri _uri;

        public VenueService(
            ILogger<VenueService> logger,
            HttpClient httpClient,
            IOptions<GetVenueByIdSettings> settings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(httpClient, nameof(httpClient));
            Throw.IfNull(settings, nameof(settings));

            _logger = logger;
            _httpClient = httpClient;

            _uri = settings.Value.ToUri();
        }
        public async Task<IResult<IGetVenueByIdResult>> GetVenueByIdAsync(IGetVenueByIdCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Venue search criteria.", criteria);
                _logger.LogInformationObject("Venue search URI", _uri);

                var content = new StringContent(criteria.ToJson(), Encoding.UTF8, "application/json");

               // var a = _uri + "&id=bb84a2e5-f4ff-445b-940b-141cdb1bcd6f";
               // var response = await _httpClient.GetAsync(a);
                
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
                    var venue = JsonConvert.DeserializeObject<GetVenueByIdResult>(json, settings);

                   
                    return Result.Ok<IGetVenueByIdResult>(venue);
                }
                else
                {
                    return Result.Fail<IGetVenueByIdResult>("Venue search service unsuccessful http response");
                }
            }

            catch (HttpRequestException hre)
            {
                _logger.LogException("Venue search service http request error", hre);
                return Result.Fail<IGetVenueByIdResult>("Venue search service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Venue search service unknown error.", e);

                return Result.Fail<IGetVenueByIdResult>("Venue search service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }

        }

    }
    internal static class GetVenueByIdSettingsExtensions
    {
        internal static Uri ToUri(this IGetVenueByIdSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + extendee.ApiKey}");
            //return new Uri($"{extendee.ApiUrl}?api-version={extendee.ApiVersion}");
        }
    }
    internal static class IGetVenueByIdCriteriaExtensions
    {
        internal static string ToJson(this IGetVenueByIdCriteria extendee)
        {

            GetVenueByIdJson json = new GetVenueByIdJson
            {
                id = extendee.Id
            };
            var result = JsonConvert.SerializeObject(json);

            return result;
        }
    }

    internal class GetVenueByIdJson
    {
        public string id { get; set; }
    }

}
