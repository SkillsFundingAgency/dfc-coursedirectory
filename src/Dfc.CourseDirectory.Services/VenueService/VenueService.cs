﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
                var venueJson = JsonConvert.SerializeObject(venue);

                var content = new StringContent(venueJson, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_updateVenueUri, content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var venueResult = JsonConvert.DeserializeObject<Venue>(json);

                    return Result.Ok(venueResult);
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

        public async Task<Result<Venue>> GetVenueByIdAsync(GetVenueByIdCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));

            try
            {
                var response = await _httpClient.GetAsync(_getVenueByIdUri.AbsoluteUri + $"?id={criteria.Id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new VenueSearchResultContractResolver()
                    };
                    var venue = JsonConvert.DeserializeObject<Venue>(json, settings);


                    return Result.Ok(venue);
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

        public async Task<Result<VenueSearchResult>> GetVenuesByPRNAndNameAsync(GetVenuesByPRNAndNameCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));

            try
            {
                var response = await _httpClient.GetAsync(_getVenueByPRNAndNameUri + $"?prn={criteria.PRN}&name={criteria.Name}");

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(json))
                        json = "[]";

                    JsonSerializerSettings settings = new JsonSerializerSettings
                    {
                        ContractResolver = new VenueSearchResultContractResolver()
                    };
                    IEnumerable<Venue> venues = JsonConvert.DeserializeObject<IEnumerable<Venue>>(json, settings);
                    return Result.Ok(new VenueSearchResult(venues));

                }
                else
                {
                    return Result.Fail<VenueSearchResult>("Get Venue By PRN & Name service unsuccessful http response");
                }
            }

            catch (HttpRequestException hre)
            {
                _logger.LogException("Get Venue By PRN and Name service http request error", hre);
                return Result.Fail<VenueSearchResult>("Get Venue By PRN and Name service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Get Venue By PRN and Name service unknown error.", e);
                return Result.Fail<VenueSearchResult>("Get Venue By PRN and Name service unknown error.");
            }
        }

        public async Task<Result<VenueSearchResult>> SearchAsync(VenueSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));

            try
            {
                var response = await _httpClient.GetAsync(_searchVenueUri + $"?prn={criteria.Search}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return Result.Ok(new VenueSearchResult(new List<Venue>()));
                }

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new VenueSearchResultContractResolver()
                    };
                    var venues = JsonConvert.DeserializeObject<IEnumerable<Venue>>(json, settings).OrderBy(x => x.VenueName).ToList();

                    if (!string.IsNullOrEmpty(criteria.NewAddressId))
                    {
                        var newVenueIndex = venues.FindIndex(x => x.ID == criteria.NewAddressId);
                        var newVenueItem = venues[newVenueIndex];

                        venues.RemoveAt(newVenueIndex);
                        venues.Insert(0, newVenueItem);
                    }

                    return Result.Ok(new VenueSearchResult(venues));
                }
                else
                {
                    return Result.Fail<VenueSearchResult>("Venue search service unsuccessful http response");
                }
            }

            catch (HttpRequestException hre)
            {
                _logger.LogException("Venue search service http request error", hre);
                return Result.Fail<VenueSearchResult>("Venue search service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Venue search service unknown error.", e);

                return Result.Fail<VenueSearchResult>("Venue search service unknown error.");
            }
        }

        public async Task<Result<Venue>> AddAsync(Venue venue)
        {
            Throw.IfNull(venue, nameof(venue));

            try
            {
                var venueJson = JsonConvert.SerializeObject(venue);

                var content = new StringContent(venueJson, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_addVenueUri, content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var venueResult = JsonConvert.DeserializeObject<Venue>(json);

                    return Result.Ok(venueResult);
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
}
