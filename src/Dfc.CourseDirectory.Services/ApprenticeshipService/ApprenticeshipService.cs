using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
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

        public async Task<IResult<IEnumerable<StandardsAndFrameworks>>> StandardsAndFrameworksSearch(string criteria, int UKPRN)
        {
            Throw.IfNullOrWhiteSpace(criteria, nameof(criteria));

            try
            {
                _logger.LogInformationObject("Standards and Frameworks Criteria", criteria);

                
                var response = await _httpClient.GetAsync(new Uri(_getStandardsAndFrameworksUri.AbsoluteUri + "?search=" + criteria + "&UKPRN=" + UKPRN));
                _logger.LogHttpResponseMessage("Standards and Frameworks service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Get your apprenticeship service json response", json);
                    IEnumerable<StandardsAndFrameworks> results = JsonConvert.DeserializeObject<IEnumerable<StandardsAndFrameworks>>(json);

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

        public async Task<IResult> AddApprenticeship(Apprenticeship apprenticeship)
        {
            Throw.IfNull(apprenticeship, nameof(apprenticeship));

            try
            {
                _logger.LogInformationObject("Apprenticeship add object.", apprenticeship);
                _logger.LogInformationObject("Apprenticeship  add URI", _addApprenticeshipUri);

                var apprenticeshipJson = JsonConvert.SerializeObject(apprenticeship);

                var content = new StringContent(apprenticeshipJson, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(_addApprenticeshipUri, content);

                _logger.LogHttpResponseMessage("Apprenticeship add service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Apprenticeship add service json response", json);


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
                return Result.Fail<Apprenticeship>("Apprenticeship add service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Apprenticeship add service unknown error.", e);

                return Result.Fail<Apprenticeship>("Apprenticeship add service unknown error.");
            }
        }

        public async Task<IResult> AddApprenticeships(
            IEnumerable<Apprenticeship> apprenticeships,
            bool addInParallel)
        {
            Throw.IfNull(apprenticeships, nameof(apprenticeships));

            try
            {
                _logger.LogInformationObject("Apprenticeship add object.", apprenticeships);
                _logger.LogInformationObject("Apprenticeship  add URI", _addApprenticeshipUri);

                var apprenticeshipJson = JsonConvert.SerializeObject(apprenticeships);

                var content = new StringContent(apprenticeshipJson, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    _addApprenticeshipsUri + $"?parallel={addInParallel}",
                    content);

                _logger.LogHttpResponseMessage("Apprenticeship add service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Apprenticeship add service json response", json);


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
                return Result.Fail<Apprenticeship>("Apprenticeship add service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Apprenticeship add service unknown error.", e);

                return Result.Fail<Apprenticeship>("Apprenticeship add service unknown error.");
            }
        }

        public async Task<IResult<Apprenticeship>> GetApprenticeshipByIdAsync(string Id)
        {
            Throw.IfNullOrWhiteSpace(Id, nameof(Id));

            try
            {
                _logger.LogInformationObject("Get Apprenticeship by Id", Id);

                
                var response = await _httpClient.GetAsync(new Uri(_getApprenticeshipByIdUri.AbsoluteUri + "?id=" + Id));
                _logger.LogHttpResponseMessage("Get Apprenticeship by Id service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Get Apprenticeship by Id json response", json);
                    Apprenticeship results = JsonConvert.DeserializeObject<Apprenticeship>(json);

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

        public async Task<IResult<IEnumerable<Apprenticeship>>> GetApprenticeshipByUKPRN(string criteria)
        {
            Throw.IfNullOrWhiteSpace(criteria, nameof(criteria));

            try
            {
                _logger.LogInformationObject("Search Apprenticeship by UKPRN Criteria", criteria);

                
                var response = await _httpClient.GetAsync(new Uri(_getApprenticeshipByUKPRNUri.AbsoluteUri + "?UKPRN=" + criteria));
                _logger.LogHttpResponseMessage("Search Apprenticeship by UKPRN service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Search Apprenticeship by UKPRN json response", json);
                    IEnumerable<Apprenticeship> results = JsonConvert.DeserializeObject<IEnumerable<Apprenticeship>>(json);

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

        public async Task<IResult<IEnumerable<StandardsAndFrameworks>>> GetStandardByCode(StandardSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));

            try
            {
                _logger.LogInformationObject("StandardSearchCriteria Criteria", criteria);

                
                var response = await _httpClient.GetAsync(new Uri(_getStandardByCodeUri.AbsoluteUri + "?StandardCode=" + criteria.StandardCode + "&Version=" + criteria.Version));
                _logger.LogHttpResponseMessage("GetStandardByCode service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("GetStandardByCode service json response", json);
                    IEnumerable<StandardsAndFrameworks> results = JsonConvert.DeserializeObject<IEnumerable<StandardsAndFrameworks>>(json);

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

        public async Task<IResult<IEnumerable<StandardsAndFrameworks>>> GetFrameworkByCode(FrameworkSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));

            try
            {
                _logger.LogInformationObject("FrameworkSearchCriteria Criteria", criteria);

                
                var response = await _httpClient.GetAsync(new Uri(_getFrameworkByCodeUri.AbsoluteUri + "?FrameworkCode=" + criteria.FrameworkCode + "&ProgType=" + criteria.ProgType + "&PathwayCode=" + criteria.PathwayCode));
                _logger.LogHttpResponseMessage("GetFrameworkByCode service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("GetFrameworkByCode service json response", json);
                    IEnumerable<StandardsAndFrameworks> results = JsonConvert.DeserializeObject<IEnumerable<StandardsAndFrameworks>>(json);

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
        public async Task<IResult<Apprenticeship>> UpdateApprenticeshipAsync(Apprenticeship apprenticeship)
        {
            Throw.IfNull(apprenticeship, nameof(apprenticeship));

            try
            {
                _logger.LogInformationObject("apprenticeship update object.", apprenticeship);
                _logger.LogInformationObject("apprenticeship update URI", _updateApprenticshipUri);

                var apprenticeshipJson = JsonConvert.SerializeObject(apprenticeship);

                var content = new StringContent(apprenticeshipJson, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(_updateApprenticshipUri, content);

                _logger.LogHttpResponseMessage("Apprenticeship update service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Apprenticeship update service json response", json);

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
        public async Task<IResult> DeleteBulkUploadApprenticeships(int UKPRN)
        {
            Throw.IfLessThan(0, UKPRN, nameof(UKPRN));

            try
            {
               
                
                var response = await _httpClient.GetAsync(new Uri(_deleteBulkUploadApprenticeshipsUri.AbsoluteUri + "?UKPRN=" + UKPRN));
                _logger.LogHttpResponseMessage("Delete Bulk Upload Apprenticeship Status http response", response);

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
        public async Task<IResult> ChangeApprenticeshipStatusesForUKPRNSelection(int UKPRN, int CurrentStatus, int StatusToBeChangedTo)
        {
            Throw.IfNull(UKPRN, nameof(UKPRN));
            Throw.IfNull(CurrentStatus, nameof(CurrentStatus));
            Throw.IfNull(StatusToBeChangedTo, nameof(StatusToBeChangedTo));

            
            
            
            var response = await _httpClient.GetAsync(new Uri(_changeApprenticeshipStatusesForUKPRNSelectionUri.AbsoluteUri + "?UKPRN=" + UKPRN + "&CurrentStatus=" + CurrentStatus + "&StatusToBeChangedTo=" + StatusToBeChangedTo));
            _logger.LogHttpResponseMessage("ChangeApprenticeshipStatusesForUKPRNSelection service http response", response);

            if (response.IsSuccessStatusCode)
            {
                return Result.Ok();
            }

            return Result.Fail("ChangeApprenticeshipStatusesForUKPRNSelection service unsuccessful http response");
            
        }
        public async Task<IResult<ApprenticeshipDashboardCounts>> GetApprenticeshipDashboardCounts(int UKPRN)
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

        public async Task<IResult<IList<DfcMigrationReport>>> GetAllDfcReports()
        {
            try
            {
                _logger.LogInformationObject("Get all dfc reports URI", _getAllDfcReports);

                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.GetAsync(new Uri(_getAllDfcReports.AbsoluteUri));
                _logger.LogHttpResponseMessage("Get course migration report service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Get All Dfc migration reports service json response", json);
                    IList<DfcMigrationReport> dfcReports = JsonConvert.DeserializeObject<IList<DfcMigrationReport>>(json);
                    return Result.Ok<IList<DfcMigrationReport>>(dfcReports);
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

        public async Task<IResult<int>> GetTotalLiveApprenticeships()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);

                var response = await httpClient.GetAsync(_getTotalLiveCoursesUri);
                _logger.LogHttpResponseMessage("GetTotalLiveApprenticeships service http response", response);

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

        internal static Uri AddApprenticeshipsUri(this IApprenticeshipServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/AddApprenticeships");
        }

        internal static Uri DeleteBulkUploadApprenticeshipsUri(this IApprenticeshipServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/DeleteBulkUploadApprenticeships");
        }

        internal static Uri GetApprenticeshipByUKPRNUri(this IApprenticeshipServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetApprenticeshipByUKPRN");
        }
        internal static Uri ToGetAllDfcReports(this IApprenticeshipServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetAllDfcReports");
        }
        internal static Uri GetApprenticeshipByIdUri(this IApprenticeshipServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetApprenticeshipById");
        }
        internal static Uri GetStandardByCodeUri(this IApprenticeshipServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetStandardByCode");
        }
        internal static Uri GetFrameworkByCodeUri(this IApprenticeshipServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetFrameworkByCode");
        }
        internal static Uri UpdateAprrenticeshipUri(this IApprenticeshipServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/UpdateApprenticeship");
        }        
        internal static Uri ChangeApprenticeshipStatusesForUKPRNSelectionUri(this IApprenticeshipServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/ChangeApprenticeshipStatusForUKPRNSelection");
        }
        internal static Uri GetApprenticeshipDashboardCountsUri(this IApprenticeshipServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetApprenticeshipDashboardCounts");
        }

        internal static Uri ToGetTotalLiveCourses(this IApprenticeshipServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetTotalLiveApprenticeships");
        }
    }
}
