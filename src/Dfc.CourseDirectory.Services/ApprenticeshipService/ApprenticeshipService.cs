using Dfc.CourseDirectory.Common;
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
        private readonly HttpClient _httpClient;
        private readonly Uri _getStandardsAndFrameworksUri, _addApprenticeshipUri, _getApprenticeshipByUKPRNUri;


        public ApprenticeshipService(
            ILogger<ApprenticeshipService> logger,
            HttpClient httpClient,
            IOptions<ApprenticeshipServiceSettings> settings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(httpClient, nameof(httpClient));
            Throw.IfNull(settings, nameof(settings));

            _logger = logger;
            _httpClient = httpClient;

            _getStandardsAndFrameworksUri = settings.Value.GetStandardsAndFrameworksUri();
            _addApprenticeshipUri = settings.Value.AddApprenticeshipUri();
            _getApprenticeshipByUKPRNUri = settings.Value.GetApprenticeshipByUKPRNUri();

        }

        public async Task<IResult<IEnumerable<IStandardsAndFrameworks>>> StandardsAndFrameworksSearch(string criteria)
        {
            Throw.IfNullOrWhiteSpace(criteria, nameof(criteria));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Standards and Frameworks Criteria", criteria);


                var response = await _httpClient.GetAsync(new Uri(_getStandardsAndFrameworksUri.AbsoluteUri + "&search=" + criteria));
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

        public async Task<IResult<IEnumerable<IApprenticeship>>> GetApprenticeshipByUKPRN(string criteria)
        {
            Throw.IfNullOrWhiteSpace(criteria, nameof(criteria));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Search Apprenticeship by UKPRN Criteria", criteria);


                var response = await _httpClient.GetAsync(new Uri(_getApprenticeshipByUKPRNUri.AbsoluteUri + "&UKPRN=" + criteria));
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
    }
    internal static class ApprenticeshipServiceSettingsExtensions
    {
        internal static Uri GetStandardsAndFrameworksUri(this IApprenticeshipServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "StandardsAndFrameworksSearch?code=" + extendee.ApiKey}");
        }
        internal static Uri AddApprenticeshipUri(this IApprenticeshipServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "AddApprenticeship?code=" + extendee.ApiKey}");
        }
        internal static Uri GetApprenticeshipByUKPRNUri(this IApprenticeshipServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "GetApprenticeshipByUKPRN?code=" + extendee.ApiKey}");
        }
    }
    
}
