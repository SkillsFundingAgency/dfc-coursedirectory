using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Apprenticeships;
using Dfc.CourseDirectory.Services.Models.Courses;
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
        private readonly Uri _getStandardsAndFrameworksUri, _addApprenticeshipUri, _addApprenticeshipsUri, _getApprenticeshipByUKPRNUri, 
            _getApprenticeshipByIdUri, _updateApprenticshipUri, _getStandardByCodeUri, _getFrameworkByCodeUri, _deleteBulkUploadApprenticeshipsUri,
            _changeApprenticeshipStatusesForUKPRNSelectionUri, _getApprenticeshipDashboardCountsUri, _getAllDfcReports, _getTotalLiveCoursesUri;

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
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
            _getStandardsAndFrameworksUri = settings.Value.GetStandardsAndFrameworksUri();
            _addApprenticeshipUri = settings.Value.AddApprenticeshipUri();
            _addApprenticeshipsUri = settings.Value.AddApprenticeshipsUri();
            _getApprenticeshipByUKPRNUri = settings.Value.GetApprenticeshipByUKPRNUri();
            _getApprenticeshipByIdUri = settings.Value.GetApprenticeshipByIdUri();
            _updateApprenticshipUri = settings.Value.UpdateAprrenticeshipUri();
            _getStandardByCodeUri = settings.Value.GetStandardByCodeUri();
            _getFrameworkByCodeUri = settings.Value.GetFrameworkByCodeUri();
            _deleteBulkUploadApprenticeshipsUri = settings.Value.DeleteBulkUploadApprenticeshipsUri();
            _changeApprenticeshipStatusesForUKPRNSelectionUri =
                settings.Value.ChangeApprenticeshipStatusesForUKPRNSelectionUri();
            _getApprenticeshipDashboardCountsUri = settings.Value.GetApprenticeshipDashboardCountsUri();
            _getAllDfcReports = settings.Value.ToGetAllDfcReports();
            _getTotalLiveCoursesUri = settings.Value.ToGetTotalLiveCourses();
        }

        public async Task<Result<IEnumerable<StandardsAndFrameworks>>> StandardsAndFrameworksSearch(string criteria, int UKPRN)
        {
            Throw.IfNullOrWhiteSpace(criteria, nameof(criteria));

            try
            {
                var response = await _httpClient.GetAsync(new Uri(_getStandardsAndFrameworksUri.AbsoluteUri + "?search=" + criteria + "&UKPRN=" + UKPRN));

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var results = JsonConvert.DeserializeObject<IEnumerable<StandardsAndFrameworks>>(json);

                    return Result.Ok<IEnumerable<StandardsAndFrameworks>>(results);
                }
                else
                {
                    return Result.Fail<IEnumerable<StandardsAndFrameworks>>("Standards and Frameworks service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Get your Standards and Frameworks service http request error", hre);
                return Result.Fail<IEnumerable<StandardsAndFrameworks>>("Standards and Frameworks service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Standards and Frameworks unknown error.", e);
                return Result.Fail<IEnumerable<StandardsAndFrameworks>>("Standards and Frameworks service unknown error.");
            }
        }

        public async Task<Result> AddApprenticeship(Apprenticeship apprenticeship)
        {
            Throw.IfNull(apprenticeship, nameof(apprenticeship));

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
                _logger.LogException("Apprenticeship add service http request error", hre);
                return Result.Fail("Apprenticeship add service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Apprenticeship add service unknown error.", e);

                return Result.Fail("Apprenticeship add service unknown error.");
            }
        }

        public async Task<Result> AddApprenticeships(
            IEnumerable<Apprenticeship> apprenticeships,
            bool addInParallel)
        {
            Throw.IfNull(apprenticeships, nameof(apprenticeships));

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
                _logger.LogException("Apprenticeship add service http request error", hre);
                return Result.Fail("Apprenticeship add service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Apprenticeship add service unknown error.", e);

                return Result.Fail("Apprenticeship add service unknown error.");
            }
        }

        public async Task<Result<Apprenticeship>> GetApprenticeshipByIdAsync(string Id)
        {
            Throw.IfNullOrWhiteSpace(Id, nameof(Id));

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
                _logger.LogException("Get Apprenticeship by Id service http request error", hre);
                return Result.Fail<Apprenticeship>("Get Apprenticeship by Id service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Get apprenticeship unknown error.", e);
                return Result.Fail<Apprenticeship>("Get Apprenticeship by Id service unknown error.");
            }
        }

        public async Task<Result<IEnumerable<Apprenticeship>>> GetApprenticeshipByUKPRN(string criteria)
        {
            Throw.IfNullOrWhiteSpace(criteria, nameof(criteria));

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
                _logger.LogException("Search Apprenticeship by UKPRN service http request error", hre);
                return Result.Fail<IEnumerable<Apprenticeship>>("Search Apprenticeship by UKPRN service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Standards and Frameworks unknown error.", e);
                return Result.Fail<IEnumerable<Apprenticeship>>("Search Apprenticeship by UKPRN service unknown error.");
            }
        }

        public async Task<Result<IEnumerable<StandardsAndFrameworks>>> GetStandardByCode(StandardSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));

            try
            {
                var response = await _httpClient.GetAsync(new Uri(_getStandardByCodeUri.AbsoluteUri + "?StandardCode=" + criteria.StandardCode + "&Version=" + criteria.Version));

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var results = JsonConvert.DeserializeObject<IEnumerable<StandardsAndFrameworks>>(json);

                    return Result.Ok<IEnumerable<StandardsAndFrameworks>>(results);
                }
                else
                {
                    return Result.Fail<IEnumerable<StandardsAndFrameworks>>("GetStandardByCode service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("GetStandardByCode service http request error", hre);
                return Result.Fail<IEnumerable<StandardsAndFrameworks>>("GetStandardByCode service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("GetStandardByCode unknown error.", e);
                return Result.Fail<IEnumerable<StandardsAndFrameworks>>("GetStandardByCode service unknown error.");
            }
        }

        public async Task<Result<IEnumerable<StandardsAndFrameworks>>> GetFrameworkByCode(FrameworkSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));

            try
            {
                var response = await _httpClient.GetAsync(new Uri(_getFrameworkByCodeUri.AbsoluteUri + "?FrameworkCode=" + criteria.FrameworkCode + "&ProgType=" + criteria.ProgType + "&PathwayCode=" + criteria.PathwayCode));

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var results = JsonConvert.DeserializeObject<IEnumerable<StandardsAndFrameworks>>(json);

                    return Result.Ok<IEnumerable<StandardsAndFrameworks>>(results);
                }
                else
                {
                    return Result.Fail<IEnumerable<StandardsAndFrameworks>>("GetFrameworkByCode service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("GetFrameworkByCode service http request error", hre);
                return Result.Fail<IEnumerable<StandardsAndFrameworks>>("GetFrameworkByCode service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("GetStandardByCode unknown error.", e);
                return Result.Fail<IEnumerable<StandardsAndFrameworks>>("GetFrameworkByCode service unknown error.");
            }
        }

        public async Task<Result<Apprenticeship>> UpdateApprenticeshipAsync(Apprenticeship apprenticeship)
        {
            Throw.IfNull(apprenticeship, nameof(apprenticeship));

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
                _logger.LogException("Apprenticeship update service http request error", hre);
                return Result.Fail<Apprenticeship>("Apprenticeship update service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Apprenticeship update service unknown error.", e);

                return Result.Fail<Apprenticeship>("Apprenticeship update service unknown error.");
            }
        }

        public async Task<Result> DeleteBulkUploadApprenticeships(int UKPRN)
        {
            Throw.IfLessThan(0, UKPRN, nameof(UKPRN));

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
            Throw.IfNull(UKPRN, nameof(UKPRN));
            Throw.IfNull(CurrentStatus, nameof(CurrentStatus));
            Throw.IfNull(StatusToBeChangedTo, nameof(StatusToBeChangedTo));
            
            var response = await _httpClient.GetAsync(new Uri(_changeApprenticeshipStatusesForUKPRNSelectionUri.AbsoluteUri + "?UKPRN=" + UKPRN + "&CurrentStatus=" + CurrentStatus + "&StatusToBeChangedTo=" + StatusToBeChangedTo));

            if (response.IsSuccessStatusCode)
            {
                return Result.Ok();
            }

            return Result.Fail("ChangeApprenticeshipStatusesForUKPRNSelection service unsuccessful http response");
        }

        public async Task<Result<ApprenticeshipDashboardCounts>> GetApprenticeshipDashboardCounts(int UKPRN)
        {
            Throw.IfLessThan(0, UKPRN, nameof(UKPRN));

            try
            {
                var response = await _httpClient.GetAsync(new Uri(_getApprenticeshipDashboardCountsUri.AbsoluteUri + "?UKPRN=" + UKPRN));

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var results = JsonConvert.DeserializeObject<ApprenticeshipDashboardCounts>(json);

                    return Result.Ok(results);
                }
                else
                {
                    return Result.Fail<ApprenticeshipDashboardCounts>("GetApprenticeshipDashboardCounts unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Get Apprenticeship by Id service http request error", hre);
                return Result.Fail<ApprenticeshipDashboardCounts>("GetApprenticeshipDashboardCounts http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Get apprenticeship unknown error.", e);
                return Result.Fail<ApprenticeshipDashboardCounts>("GetApprenticeshipDashboardCounts unknown error.");
            }
        }

        public async Task<Result<IList<DfcMigrationReport>>> GetAllDfcReports()
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.GetAsync(new Uri(_getAllDfcReports.AbsoluteUri));

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var dfcReports = JsonConvert.DeserializeObject<IList<DfcMigrationReport>>(json);
                    return Result.Ok(dfcReports);
                }
                else
                {
                    return Result.Fail<IList<DfcMigrationReport>>("Get All Dfc migration report service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Get course migration report service http request error", hre);
                return Result.Fail<IList<DfcMigrationReport>>("Get All Dfc migration report service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Get course migration report service unknown error.", e);
                return Result.Fail<IList<DfcMigrationReport>>("Get All Dfc migration report service unknown error.");
            }
        }

        public async Task<Result<int>> GetTotalLiveApprenticeships()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);

                var response = await httpClient.GetAsync(_getTotalLiveCoursesUri);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var total = JsonConvert.DeserializeObject<int>(json);

                    return Result.Ok(total);
                }
                else
                {
                    return Result.Fail<int>("GetTotalLiveApprenticeships service unsuccessful http response");
                }
            }
        }
    }
}
