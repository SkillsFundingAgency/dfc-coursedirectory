﻿using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Services.ApprenticeshipService
{
    public class ApprenticeshipService : IApprenticeshipService
    {
        private readonly ILogger<ApprenticeshipService> _logger;
        private readonly ApprenticeshipServiceSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly Uri _getStandardsAndFrameworksUri, _addApprenticeshipUri, _getApprenticeshipByUKPRNUri, _getApprenticeshipByIdUri, _updateApprenticshipUri;

        public ApprenticeshipService(
            ILogger<ApprenticeshipService> logger,
            HttpClient httpClient,
            IOptions<ApprenticeshipServiceSettings> settings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(httpClient, nameof(httpClient));
            Throw.IfNull(settings, nameof(settings));

            _logger = logger;
            _settings = settings.Value;
            _httpClient = httpClient;

            _getStandardsAndFrameworksUri = settings.Value.GetStandardsAndFrameworksUri();
            _addApprenticeshipUri = settings.Value.AddApprenticeshipUri();
            _getApprenticeshipByUKPRNUri = settings.Value.GetApprenticeshipByUKPRNUri();
            _getApprenticeshipByIdUri = settings.Value.GetApprenticeshipByIdUri();
            _updateApprenticshipUri = settings.Value.UpdateAprrenticeshipUri();

        }

        public async Task<IResult<IEnumerable<IStandardsAndFrameworks>>> StandardsAndFrameworksSearch(string criteria)
        {
            Throw.IfNullOrWhiteSpace(criteria, nameof(criteria));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Standards and Frameworks Criteria", criteria);

                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.GetAsync(new Uri(_getStandardsAndFrameworksUri.AbsoluteUri + "?search=" + criteria));
                _logger.LogHttpResponseMessage("Standards and Frameworks service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Get your apprenticeship service json response", json);
                    IEnumerable<StandardsAndFrameworks> results = JsonConvert.DeserializeObject<IEnumerable<StandardsAndFrameworks>>(json);

                    return Result.Ok<IEnumerable<IStandardsAndFrameworks>>(results);

                }
                else
                {
                    return Result.Fail<IEnumerable<IStandardsAndFrameworks>>("Standards and Frameworks service unsuccessful http response");
                }

            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Get your Standards and Frameworks service http request error", hre);
                return Result.Fail<IEnumerable<IStandardsAndFrameworks>>("Standards and Frameworks service http request error.");

            }
            catch (Exception e)
            {
                _logger.LogException("Standards and Frameworks unknown error.", e);
                return Result.Fail<IEnumerable<IStandardsAndFrameworks>>("Standards and Frameworks service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }
        public async Task<IResult<IApprenticeship>> AddApprenticeship(IApprenticeship apprenticeship)
        {
            _logger.LogMethodEnter();
            Throw.IfNull(apprenticeship, nameof(apprenticeship));

            try
            {
                _logger.LogInformationObject("Apprenticeship add object.", apprenticeship);
                _logger.LogInformationObject("Apprenticeship add URI", _addApprenticeshipUri);

                var apprenticeshipJson = JsonConvert.SerializeObject(apprenticeship);

                var content = new StringContent(apprenticeshipJson, Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.PostAsync(_addApprenticeshipUri, content);

                _logger.LogHttpResponseMessage("Apprenticeship add service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Apprenticeship add service json response", json);


                    var apprenticeshipResult = JsonConvert.DeserializeObject<Apprenticeship>(json);


                    return Result.Ok<IApprenticeship>(apprenticeshipResult);
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return Result.Fail<IApprenticeship>("Apprenticeship add service unsuccessful http response - TooManyRequests");
                }
                else
                {
                    return Result.Fail<IApprenticeship>("Apprenticeship add service unsuccessful http response - ResponseStatusCode: " + response.StatusCode);
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Apprenticeship add service http request error", hre);
                return Result.Fail<IApprenticeship>("Apprenticeship add service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Apprenticeship add service unknown error.", e);

                return Result.Fail<IApprenticeship>("Apprenticeship add service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }
        public async Task<IResult<IApprenticeship>> GetApprenticeshipByIdAsync(string Id)
        {
            Throw.IfNullOrWhiteSpace(Id, nameof(Id));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Get Apprenticeship by Id", Id);

                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.GetAsync(new Uri(_getApprenticeshipByIdUri.AbsoluteUri + "?id=" + Id));
                _logger.LogHttpResponseMessage("Get Apprenticeship by Id service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Get Apprenticeship by Id json response", json);
                    Apprenticeship results = JsonConvert.DeserializeObject<Apprenticeship>(json);

                    return Result.Ok<IApprenticeship>(results);

                }
                else
                {
                    return Result.Fail<IApprenticeship>("Get Apprenticeship by Id service unsuccessful http response");
                }

            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Get Apprenticeship by Id service http request error", hre);
                return Result.Fail<IApprenticeship>("Get Apprenticeship by Id service http request error.");

            }
            catch (Exception e)
            {
                _logger.LogException("Get apprenticeship unknown error.", e);
                return Result.Fail<IApprenticeship>("Get Apprenticeship by Id service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }
        public async Task<IResult<IEnumerable<IApprenticeship>>> GetApprenticeshipByUKPRN(string criteria)
        {
            Throw.IfNullOrWhiteSpace(criteria, nameof(criteria));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Search Apprenticeship by UKPRN Criteria", criteria);

                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.GetAsync(new Uri(_getApprenticeshipByUKPRNUri.AbsoluteUri + "?UKPRN=" + criteria));
                _logger.LogHttpResponseMessage("Search Apprenticeship by UKPRN service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Search Apprenticeship by UKPRN json response", json);
                    IEnumerable<Apprenticeship> results = JsonConvert.DeserializeObject<IEnumerable<Apprenticeship>>(json);

                    return Result.Ok<IEnumerable<IApprenticeship>>(results);

                }
                else
                {
                    return Result.Fail<IEnumerable<IApprenticeship>>("Search Apprenticeship by UKPRN service unsuccessful http response");
                }

            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Search Apprenticeship by UKPRN service http request error", hre);
                return Result.Fail<IEnumerable<IApprenticeship>>("Search Apprenticeship by UKPRN service http request error.");

            }
            catch (Exception e)
            {
                _logger.LogException("Standards and Frameworks unknown error.", e);
                return Result.Fail<IEnumerable<IApprenticeship>>("Search Apprenticeship by UKPRN service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }
        public async Task<IResult<IApprenticeship>> UpdateApprenticeshipAsync(IApprenticeship apprenticeship)
        {
            _logger.LogMethodEnter();
            Throw.IfNull(apprenticeship, nameof(apprenticeship));

            try
            {
                _logger.LogInformationObject("apprenticeship update object.", apprenticeship);
                _logger.LogInformationObject("apprenticeship update URI", _updateApprenticshipUri);

                var apprenticeshipJson = JsonConvert.SerializeObject(apprenticeship);

                var content = new StringContent(apprenticeshipJson, Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.PostAsync(_updateApprenticshipUri, content);

                _logger.LogHttpResponseMessage("Apprenticeship update service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Apprenticeship update service json response", json);

                    var apprenticeshipResult = JsonConvert.DeserializeObject<Apprenticeship>(json);

                    return Result.Ok<IApprenticeship>(apprenticeshipResult);
                }
                else
                {
                    return Result.Fail<IApprenticeship>("Appprenticeship update service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Apprenticeship update service http request error", hre);
                return Result.Fail<IApprenticeship>("Apprenticeship update service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Apprenticeship update service unknown error.", e);

                return Result.Fail<IApprenticeship>("Apprenticeship update service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }
    }
    internal static class ApprenticeshipServiceSettingsExtensions
    {
        internal static Uri GetStandardsAndFrameworksUri(this IApprenticeshipServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/StandardsAndFrameworksSearch");
        }

        internal static Uri AddApprenticeshipUri(this IApprenticeshipServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/AddApprenticeship");
        }

        internal static Uri GetApprenticeshipByUKPRNUri(this IApprenticeshipServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetApprenticeshipByUKPRN");
        }

        internal static Uri GetApprenticeshipByIdUri(this IApprenticeshipServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetApprenticeshipById");
        }

        internal static Uri UpdateAprrenticeshipUri(this IApprenticeshipServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/UpdateApprenticeship");
        }
    }
}
