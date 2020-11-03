using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.Models.Venues;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.Services.VenueService
{
    public class VenueService : IVenueService
    {
        private readonly ILogger<VenueService> _logger;
        private readonly VenueServiceSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly Uri _getVenueByIdUri;
        private readonly Uri _getVenueByVenueIdUri;
        private readonly Uri _getVenueByLocationIdUri;
        private readonly Uri _getVenueByPRNAndNameUri;
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
            _settings = settings.Value;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);

            _getVenueByIdUri = settings.Value.ToGetVenueByIdUri();
            _getVenueByVenueIdUri = settings.Value.ToGetVenueByVenueIdUri();
            _getVenueByLocationIdUri = settings.Value.ToGetVenueByLocationIdUri();
            _getVenueByPRNAndNameUri = settings.Value.ToGetVenuesByPRNAndNameUri();
            _updateVenueUri = settings.Value.ToUpdateVenueUrl();
            _searchVenueUri = settings.Value.ToSearchVenueUri();
            _addVenueUri = settings.Value.ToAddVenueUri();
        }

        public async Task<Result<Venue>> UpdateAsync(Venue venue)
        {
            Throw.IfNull(venue, nameof(venue));

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

                    return Result.Ok<Venue>(venueResult);
                }
                else
                {
                    return Result.Fail<Venue>("Venue update service unsuccessful http response");
                }
            }

            catch (HttpRequestException hre)
            {
                _logger.LogException("Venue update service http request error", hre);
                return Result.Fail<Venue>("Venue update service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Venue update service unknown error.", e);

                return Result.Fail<Venue>("Venue update service unknown error.");
            }
        }

        public async Task<Result<Venue>> GetVenueByIdAsync(IGetVenueByIdCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));

            try
            {
                _logger.LogInformationObject("Get Venue By Id criteria.", criteria);
                _logger.LogInformationObject("Get Venue By Id URI", _getVenueByIdUri);

                var response = await _httpClient.GetAsync(_getVenueByIdUri.AbsoluteUri + $"?id={criteria.Id}");

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


                    return Result.Ok<Venue>(venue);
                }
                else
                {
                    return Result.Fail<Venue>("Get Venue ByI d service unsuccessful http response");
                }
            }

            catch (HttpRequestException hre)
            {
                _logger.LogException("Get Venue By Id service http request error", hre);
                return Result.Fail<Venue>("Get Venue By Id service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Get Venue By Id service unknown error.", e);

                return Result.Fail<Venue>("Get Venue By Id service unknown error.");
            }
        }

        public async Task<Result<Venue>> GetVenueByVenueIdAsync(IGetVenueByVenueIdCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));

            try
            {
                _logger.LogInformationObject("Get Venue By VenueId criteria.", criteria);
                _logger.LogInformationObject("Get Venue By VenueId URI", _getVenueByVenueIdUri);

                var response = await _httpClient.GetAsync(_getVenueByVenueIdUri + $"?venueId={criteria.venueId}");
                _logger.LogHttpResponseMessage("Get Venue By VenueId service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformationObject("Get Venue By VenueId service json response", json);

                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new VenueSearchResultContractResolver()
                    };
                    var venue = JsonConvert.DeserializeObject<Venue>(json, settings);

                    return Result.Ok<Venue>(venue);

                }
                else
                {
                    return Result.Fail<Venue>("Get Venue By VenueId service unsuccessful http response");
                }

            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Get Venue By VenueId service http request error", hre);
                return Result.Fail<Venue>("Get Venue By VenueId service http request error.");

            }
            catch (Exception e)
            {
                _logger.LogException("Get Venue By VenueId service unknown error.", e);
                return Result.Fail<Venue>("Get Venue By VenueId service unknown error.");

            }
        }

        public async Task<Result<Venue>> GetVenueByLocationIdAsync(IGetVenueByLocationIdCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));

            try
            {
                _logger.LogInformationObject("Get Venue By LocationId criteria.", criteria);
                _logger.LogInformationObject("Get Venue By LocationId URI", _getVenueByLocationIdUri);

                var response = await _httpClient.GetAsync(_getVenueByLocationIdUri + $"?LocationId={criteria.LocationId}");
                _logger.LogHttpResponseMessage("Get Venue By LocationId service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformationObject("Get Venue By LocationId service json response", json);

                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new VenueSearchResultContractResolver()
                    };
                    var venue = JsonConvert.DeserializeObject<Venue>(json, settings);

                    return Result.Ok<Venue>(venue);

                }
                else
                {
                    return Result.Fail<Venue>("Get Venue By LocationId service unsuccessful http response");
                }

            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Get Venue By LocationId service http request error", hre);
                return Result.Fail<Venue>("Get Venue By LocationId service http request error.");

            }
            catch (Exception e)
            {
                _logger.LogException("Get Venue By LocationId service unknown error.", e);
                return Result.Fail<Venue>("Get Venue By LocationId service unknown error.");

            }
        }

        public async Task<Result<IVenueSearchResult>> GetVenuesByPRNAndNameAsync(IGetVenuesByPRNAndNameCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));

            try
            {
                _logger.LogInformationObject("Get Venue By PRN & Name criteria.", criteria);
                _logger.LogInformationObject("Get Venue By PRN & Name URI", _getVenueByPRNAndNameUri);

                HttpResponseMessage response = await _httpClient.GetAsync(_getVenueByPRNAndNameUri + $"?prn={criteria.PRN}&name={criteria.Name}");

                _logger.LogHttpResponseMessage("Get Venue By PRN and Name service http response", response);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformationObject("Venue search service json response", json);

                    if (string.IsNullOrEmpty(json))
                        json = "[]";

                    JsonSerializerSettings settings = new JsonSerializerSettings
                    {
                        ContractResolver = new VenueSearchResultContractResolver()
                    };
                    IEnumerable<Venue> venues = JsonConvert.DeserializeObject<IEnumerable<Venue>>(json, settings);
                    return Result.Ok<IVenueSearchResult>(new VenueSearchResult(venues));

                }
                else
                {
                    return Result.Fail<IVenueSearchResult>("Get Venue By PRN & Name service unsuccessful http response");
                }
            }

            catch (HttpRequestException hre)
            {
                _logger.LogException("Get Venue By PRN and Name service http request error", hre);
                return Result.Fail<IVenueSearchResult>("Get Venue By PRN and Name service http request error.");

            }
            catch (Exception e)
            {
                _logger.LogException("Get Venue By PRN and Name service unknown error.", e);
                return Result.Fail<IVenueSearchResult>("Get Venue By PRN and Name service unknown error.");

            }
        }

        public async Task<Result<IVenueSearchResult>> SearchAsync(IVenueSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));

            try
            {
                _logger.LogInformationObject("Venue search criteria.", criteria);
                _logger.LogInformationObject("Venue search URI", _searchVenueUri);

                var response = await _httpClient.GetAsync(_searchVenueUri + $"?prn={criteria.Search}");

                _logger.LogHttpResponseMessage("Venue search service http response", response);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    return Result.Ok<IVenueSearchResult>(new VenueSearchResult(new List<Venue>()));

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

                    var searchResult = new VenueSearchResult(venues);
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
        }

        public async Task<Result<Venue>> AddAsync(Venue venue)
        {
            Throw.IfNull(venue, nameof(venue));

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

                    return Result.Ok<Venue>(venueResult);
                }
                else
                {
                    return Result.Fail<Venue>("Venue add service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Venue add service http request error", hre);
                return Result.Fail<Venue>("Venue add service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Venue add service unknown error.", e);

                return Result.Fail<Venue>("Venue add service unknown error.");
            }
        }
    }

    internal static class VenueServiceSettingsExtensions
    {
        internal static Uri ToUpdateVenueUrl(this VenueServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/UpdateVenueById");
        }

        internal static Uri ToGetVenueByIdUri(this VenueServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetVenueById");
        }

        internal static Uri ToGetVenueByVenueIdUri(this VenueServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetVenueByVenueId");
        }

        internal static Uri ToGetVenueByLocationIdUri(this VenueServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetVenueByLocationId");
        }

        internal static Uri ToGetVenuesByPRNAndNameUri(this VenueServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetVenuesByPRNAndName");
        }

        internal static Uri ToSearchVenueUri(this VenueServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetVenuesByPRN");
        }

        internal static Uri ToAddVenueUri(this VenueServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/AddVenue");
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

    internal static class IGetVenueByPRNAndNameCriteriaExtensions
    {
        internal static string ToJson(this IGetVenuesByPRNAndNameCriteria extendee)
        {
            GetVenueByPRNAndNameJson json = new GetVenueByPRNAndNameJson
            {
                PRN = extendee.PRN,
                Name = extendee.Name
            };
            return JsonConvert.SerializeObject(json);
        }
    }

    internal class GetVenueByPRNAndNameJson
    {
        public string PRN { get; set; }
        public string Name { get; set; }
    }

    internal static class IGetVenueByVenueIdCriteriaExtensions
    {
        internal static string ToJson(this IGetVenueByVenueIdCriteria extendee)
        {

            GetVenueByVenueIdJson json = new GetVenueByVenueIdJson
            {
                venueId = extendee.venueId
            };
            var result = JsonConvert.SerializeObject(json);

            return result;
        }
    }

    internal class GetVenueByVenueIdJson
    {
        public int venueId { get; set; }
    }

    internal static class GetVenueByLocationIdCriteriaExtensions
    {
        internal static string ToJson(this IGetVenueByLocationIdCriteria extendee)
        {
            GetVenueByLocationIdJson json = new GetVenueByLocationIdJson
            {
                LocationId = extendee.LocationId
            };
            return JsonConvert.SerializeObject(json);
        }
    }

    internal class GetVenueByLocationIdJson
    {
        public int LocationId { get; set; }
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
