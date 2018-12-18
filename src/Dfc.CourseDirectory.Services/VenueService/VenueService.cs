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
using Dfc.CourseDirectory.Models.Interfaces.Venues;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;

namespace Dfc.CourseDirectory.Services.VenueService
{
    public class VenueService : IVenueService
    {
        private readonly ILogger<VenueService> _logger;
        private readonly HttpClient _httpClient;
        private readonly Uri _getVenueByIdUri;
        private readonly Uri _updateVenueUri;
        private readonly Uri _searchVenueUri;
        private readonly Uri _addVenueUri;

        public VenueService(
            ILogger<VenueService> logger,
            HttpClient httpClient,
            IOptions<VenueServiceSettings> settings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(httpClient, nameof(httpClient));
            Throw.IfNull(settings, nameof(settings));

            _logger = logger;
            _httpClient = httpClient;

            _getVenueByIdUri = settings.Value.ToGetVenueByIdUri();
            _updateVenueUri = settings.Value.ToUpdateVenueUrl();
            _searchVenueUri = settings.Value.ToSearchVenueUri();
            _addVenueUri = settings.Value.ToAddVenueUri();
        }

        public async Task<IResult<IVenue>> UpdateAsync(IVenue venue)
        {
            Throw.IfNull(venue, nameof(venue));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Venue update object.", venue);
                _logger.LogInformationObject("Venue update URI", _updateVenueUri);

                var venueJson = JsonConvert.SerializeObject(venue);

                var content = new StringContent(venueJson, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_updateVenueUri, content);

                _logger.LogHttpResponseMessage("Venue add service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Venue update service json response", json);

                    var venueResult = JsonConvert.DeserializeObject<Venue>(json);

                    return Result.Ok<IVenue>(venueResult);
                }
                else
                {
                    return Result.Fail<IVenue>("Venue update service unsuccessful http response");
                }
            }

            catch (HttpRequestException hre)
            {
                _logger.LogException("Venue update service http request error", hre);
                return Result.Fail<IVenue>("Venue update service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Venue update service unknown error.", e);

                return Result.Fail<IVenue>("Venue update service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }

        }


        public async Task<IResult<IVenue>> GetVenueByIdAsync(IGetVenueByIdCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Get Venue By Id criteria.", criteria);
                _logger.LogInformationObject("Get Venue By Id URI", _getVenueByIdUri);

                var content = new StringContent(criteria.ToJson(), Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(_getVenueByIdUri, content);

                _logger.LogHttpResponseMessage("Get Venue By Id service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Get Venue By Id service json response", json);

                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new VenueSearchResultContractResolver()
                    };
                    var venue = JsonConvert.DeserializeObject<Venue>(json, settings);

                   
                    return Result.Ok<IVenue>(venue);
                }
                else
                {
                    return Result.Fail<IVenue>("Get Venue ByI d service unsuccessful http response");
                }
            }

            catch (HttpRequestException hre)
            {
                _logger.LogException("Get Venue By Id service http request error", hre);
                return Result.Fail<IVenue>("Get Venue By Id service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Get Venue By Id service unknown error.", e);

                return Result.Fail<IVenue>("Get Venue By Id service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }

        }

        public async Task<IResult<IVenueSearchResult>> SearchAsync(IVenueSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Venue search criteria.", criteria);
                _logger.LogInformationObject("Venue search URI", _searchVenueUri);

                var content = new StringContent(criteria.ToJson(), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_searchVenueUri, content);

                _logger.LogHttpResponseMessage("Venue search service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Venue search service json response", json);

                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new VenueSearchResultContractResolver()
                    };
                    var venues = JsonConvert.DeserializeObject<IEnumerable<Venue>>(json, settings).OrderBy(x => x.VenueName).ToList();


                    if (!String.IsNullOrEmpty(criteria.NewAddressId))
                    {
                        var newVenueIndex = venues.FindIndex(x => x.ID == criteria.NewAddressId);
                        var newVenueItem = venues[newVenueIndex];

                        venues.RemoveAt(newVenueIndex);
                        venues.Insert(0, newVenueItem);
                    }

                    var searchResult = new VenueSearchResult(venues)
                    {
                        Value = venues
                    };

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

        public async Task<IResult<IVenue>> AddAsync(IVenue venue)
        {
            Throw.IfNull(venue, nameof(venue));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Venue add object.", venue);
                _logger.LogInformationObject("Venue search URI", _addVenueUri);

                var venueJson = JsonConvert.SerializeObject(venue);

                var content = new StringContent(venueJson, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_addVenueUri, content);

                _logger.LogHttpResponseMessage("Venue add service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Venue add service json response", json);

                    var venueResult = JsonConvert.DeserializeObject<Venue>(json);

                    return Result.Ok<IVenue>(venueResult);
                }
                else
                {
                    return Result.Fail<IVenue>("Venue add service unsuccessful http response");
                }
            }

            catch (HttpRequestException hre)
            {
                _logger.LogException("Venue add service http request error", hre);
                return Result.Fail<IVenue>("Venue add service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Venue add service unknown error.", e);

                return Result.Fail<IVenue>("Venue add service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }

        }
    }
    
    internal static class VenueServiceSettingsExtensions
    {
        internal static Uri ToUpdateVenueUrl(this VenueServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "UpdateVenueById?code=" + extendee.ApiKey}");
        }

        internal static Uri ToGetVenueByIdUri(this VenueServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "getvenuebyid?code=" + extendee.ApiKey}");
        }

        internal static Uri ToSearchVenueUri(this VenueServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "GetVenuesByPRN?code=" + extendee.ApiKey}");
        }

        internal static Uri ToAddVenueUri(this VenueServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "addvenue?code=" + extendee.ApiKey}");
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
