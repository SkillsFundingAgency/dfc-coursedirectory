using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Apprenticeships;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.Services.ApprenticeshipService
{
    public class ApprenticeshipService : IApprenticeshipService
    {
        private readonly ILogger<ApprenticeshipService> _logger;
        private readonly ApprenticeshipServiceSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly Uri _addApprenticeshipUri, _addApprenticeshipsUri, _getApprenticeshipByUKPRNUri, 
            _getApprenticeshipByIdUri, _updateApprenticshipUri, _deleteBulkUploadApprenticeshipsUri,
            _changeApprenticeshipStatusesForUKPRNSelectionUri;

        public ApprenticeshipService(
            ILogger<ApprenticeshipService> logger,
            HttpClient httpClient,
            IOptions<ApprenticeshipServiceSettings> settings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _logger = logger;
            _settings = settings.Value;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
            _addApprenticeshipUri = settings.Value.AddApprenticeshipUri();
            _addApprenticeshipsUri = settings.Value.AddApprenticeshipsUri();
            _getApprenticeshipByUKPRNUri = settings.Value.GetApprenticeshipByUKPRNUri();
            _getApprenticeshipByIdUri = settings.Value.GetApprenticeshipByIdUri();
            _updateApprenticshipUri = settings.Value.UpdateAprrenticeshipUri();
            _deleteBulkUploadApprenticeshipsUri = settings.Value.DeleteBulkUploadApprenticeshipsUri();
            _changeApprenticeshipStatusesForUKPRNSelectionUri = settings.Value.ChangeApprenticeshipStatusesForUKPRNSelectionUri();
        }

        public async Task<Result> AddApprenticeship(Apprenticeship apprenticeship)
        {
            if (apprenticeship == null)
            {
                throw new ArgumentNullException(nameof(apprenticeship));
            }

            try
            {
                var apprenticeshipJson = JsonConvert.SerializeObject(apprenticeship);

                var content = new StringContent(apprenticeshipJson, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(_addApprenticeshipUri, content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    return Result.Ok();
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return Result.Fail("Apprenticeship add service unsuccessful http response - TooManyRequests");
                }
                else
                {
                    return Result.Fail("Apprenticeship add service unsuccessful http response - ResponseStatusCode: " + response.StatusCode);
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Apprenticeship add service http request error");
                return Result.Fail("Apprenticeship add service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Apprenticeship add service unknown error.");

                return Result.Fail("Apprenticeship add service unknown error.");
            }
        }

        public async Task<Result> AddApprenticeships(
            IEnumerable<Apprenticeship> apprenticeships,
            bool addInParallel)
        {
            if (apprenticeships == null)
            {
                throw new ArgumentNullException(nameof(apprenticeships));
            }

            try
            {
                var apprenticeshipJson = JsonConvert.SerializeObject(apprenticeships);

                var content = new StringContent(apprenticeshipJson, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    _addApprenticeshipsUri + $"?parallel={addInParallel}",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    return Result.Ok();
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return Result.Fail("Apprenticeship add service unsuccessful http response - TooManyRequests");
                }
                else
                {
                    return Result.Fail("Apprenticeship add service unsuccessful http response - ResponseStatusCode: " + response.StatusCode);
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Apprenticeship add service http request error");
                return Result.Fail("Apprenticeship add service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Apprenticeship add service unknown error.");
                return Result.Fail("Apprenticeship add service unknown error.");
            }
        }

        public async Task<Result<Apprenticeship>> GetApprenticeshipByIdAsync(string Id)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                throw new ArgumentNullException($"{nameof(Id)} cannot be null or empty or whitespace.", nameof(Id));
            }

            try
            {
                var response = await _httpClient.GetAsync(new Uri(_getApprenticeshipByIdUri.AbsoluteUri + "?id=" + Id));

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var results = JsonConvert.DeserializeObject<Apprenticeship>(json);

                    return Result.Ok<Apprenticeship>(results);
                }
                else
                {
                    return Result.Fail<Apprenticeship>("Get Apprenticeship by Id service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Get Apprenticeship by Id service http request error");
                return Result.Fail<Apprenticeship>("Get Apprenticeship by Id service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Get apprenticeship unknown error.");
                return Result.Fail<Apprenticeship>("Get Apprenticeship by Id service unknown error.");
            }
        }

        public async Task<Result<IEnumerable<Apprenticeship>>> GetApprenticeshipByUKPRN(string criteria)
        {
            if (string.IsNullOrWhiteSpace(criteria))
            {
                throw new ArgumentNullException($"{nameof(criteria)} cannot be null or empty or whitespace.", nameof(criteria));
            }

            try
            {
                var response = await _httpClient.GetAsync(new Uri(_getApprenticeshipByUKPRNUri.AbsoluteUri + "?UKPRN=" + criteria));

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var results = JsonConvert.DeserializeObject<IEnumerable<Apprenticeship>>(json);

                    return Result.Ok<IEnumerable<Apprenticeship>>(results);
                }
                else
                {
                    return Result.Fail<IEnumerable<Apprenticeship>>("Search Apprenticeship by UKPRN service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Search Apprenticeship by UKPRN service http request error");
                return Result.Fail<IEnumerable<Apprenticeship>>("Search Apprenticeship by UKPRN service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Standards and Frameworks unknown error.");
                return Result.Fail<IEnumerable<Apprenticeship>>("Search Apprenticeship by UKPRN service unknown error.");
            }
        }

        public async Task<Result<Apprenticeship>> UpdateApprenticeshipAsync(Apprenticeship apprenticeship)
        {
            if (apprenticeship == null)
            {
                throw new ArgumentNullException(nameof(apprenticeship));
            }

            try
            {
                var apprenticeshipJson = JsonConvert.SerializeObject(apprenticeship);

                var content = new StringContent(apprenticeshipJson, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(_updateApprenticshipUri, content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var apprenticeshipResult = JsonConvert.DeserializeObject<Apprenticeship>(json);

                    return Result.Ok<Apprenticeship>(apprenticeshipResult);
                }
                else
                {
                    return Result.Fail<Apprenticeship>("Appprenticeship update service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Apprenticeship update service http request error");
                return Result.Fail<Apprenticeship>("Apprenticeship update service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Apprenticeship update service unknown error.");
                return Result.Fail<Apprenticeship>("Apprenticeship update service unknown error.");
            }
        }

        public async Task<Result> DeleteBulkUploadApprenticeships(int UKPRN)
        {
            if (UKPRN < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(UKPRN), $"{nameof(UKPRN)} cannot be less than 0.");
            }

            try
            {
                var response = await _httpClient.GetAsync(new Uri(_deleteBulkUploadApprenticeshipsUri.AbsoluteUri + "?UKPRN=" + UKPRN));

                if (response.IsSuccessStatusCode)
                {
                    return Result.Ok();
                }
                else
                {
                    return Result.Fail("Delete Bulk Upload Apprenticeship unsuccessful: " + response.ReasonPhrase);
                }
            }
            catch (Exception)
            {
                return Result.Fail("Delete Bulk Upload Apprenticeship http response");
            }
        }

        public async Task<Result> ChangeApprenticeshipStatusesForUKPRNSelection(int UKPRN, int CurrentStatus, int StatusToBeChangedTo)
        {
            var response = await _httpClient.GetAsync(new Uri(_changeApprenticeshipStatusesForUKPRNSelectionUri.AbsoluteUri + "?UKPRN=" + UKPRN + "&CurrentStatus=" + CurrentStatus + "&StatusToBeChangedTo=" + StatusToBeChangedTo));

            if (response.IsSuccessStatusCode)
            {
                return Result.Ok();
            }

            return Result.Fail("ChangeApprenticeshipStatusesForUKPRNSelection service unsuccessful http response");
        }
    }
}
