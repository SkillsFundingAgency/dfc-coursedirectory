using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Common.Settings;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Models.Models.Regions;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Dfc.CourseDirectory.Services.CourseService.CourseValidationResult;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseService : ICourseService
    {
        private readonly ILogger<CourseService> _logger;
        private readonly CourseServiceSettings _settings;
        private readonly FindACourseServiceSettings _facSettings;
        private readonly HttpClient _httpClient;
        private readonly Uri _addCourseUri;
        private readonly Uri _getYourCoursesUri;
        private readonly Uri _providerSearchUri;
        private readonly Uri _updateCourseUri;
        private readonly Uri _getCourseByIdUri;
        private readonly Uri _updateStatusUri;
        private readonly Uri _getCourseCountsByStatusForUKPRNUri;
        private readonly Uri _getRecentCourseChangesByUKPRNUri;
        private readonly Uri _changeCourseRunStatusesForUKPRNSelectionUri;
        private readonly Uri _archiveCourseRunsByUKPRNUri;
        private readonly Uri _archiveLiveCoursesUri;
        private readonly Uri _deleteBulkUploadCoursesUri;
        private readonly Uri _getCourseMigrationReportByUKPRN;
        private readonly Uri _getAllDfcReports;
        private readonly Uri _getTotalLiveCoursesUri;

        private readonly int _courseForTextFieldMaxChars;
        private readonly int _entryRequirementsTextFieldMaxChars;
        private readonly int _whatWillLearnTextFieldMaxChars;
        private readonly int _howYouWillLearnTextFieldMaxChars;
        private readonly int _whatYouNeedTextFieldMaxChars;
        private readonly int _howAssessedTextFieldMaxChars;
        private readonly int _whereNextTextFieldMaxChars;
        private readonly string _apiUserName;
        private readonly string _apiPassword;

        public CourseService(
            ILogger<CourseService> logger,
            HttpClient httpClient,
            IOptions<CourseServiceSettings> settings,
            IOptions<FindACourseServiceSettings> facSettings,
            IOptions<CourseForComponentSettings> courseForComponentSettings,
            IOptions<EntryRequirementsComponentSettings> entryRequirementsComponentSettings,
            IOptions<WhatWillLearnComponentSettings> whatWillLearnComponentSettings,
            IOptions<WhatYouNeedComponentSettings> whatYouNeedComponentSettings,
            IOptions<HowYouWillLearnComponentSettings> howYouWillLearnComponentSettings,
            IOptions<HowAssessedComponentSettings> howAssessedComponentSettings,
            IOptions<WhereNextComponentSettings> whereNextComponentSettings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(httpClient, nameof(httpClient));
            Throw.IfNull(settings, nameof(settings));
            Throw.IfNull(facSettings, nameof(facSettings));
            Throw.IfNull(courseForComponentSettings, nameof(courseForComponentSettings));
            Throw.IfNull(entryRequirementsComponentSettings, nameof(entryRequirementsComponentSettings));
            Throw.IfNull(whatWillLearnComponentSettings, nameof(whatWillLearnComponentSettings));
            Throw.IfNull(howYouWillLearnComponentSettings, nameof(howYouWillLearnComponentSettings));
            Throw.IfNull(whatYouNeedComponentSettings, nameof(whatYouNeedComponentSettings));
            Throw.IfNull(howAssessedComponentSettings, nameof(howAssessedComponentSettings));
            Throw.IfNull(whereNextComponentSettings, nameof(whereNextComponentSettings));

            _logger = logger;
            _settings = settings.Value;
            _facSettings = facSettings.Value;
            _httpClient = httpClient;

            _addCourseUri = settings.Value.ToAddCourseUri();
            _getYourCoursesUri = settings.Value.ToGetYourCoursesUri();
            _providerSearchUri = facSettings.Value.ToProviderSearchUri();
            _updateCourseUri = settings.Value.ToUpdateCourseUri();
            _getCourseByIdUri = settings.Value.ToGetCourseByIdUri();
            _archiveLiveCoursesUri = settings.Value.ToArchiveLiveCoursesUri();
            _updateStatusUri = settings.Value.ToUpdateStatusUri();
            _getCourseCountsByStatusForUKPRNUri = settings.Value.ToGetCourseCountsByStatusForUKPRNUri();
            _getRecentCourseChangesByUKPRNUri = settings.Value.ToGetRecentCourseChangesByUKPRNUri();
            _changeCourseRunStatusesForUKPRNSelectionUri = settings.Value.ToChangeCourseRunStatusesForUKPRNSelectionUri();
            _archiveCourseRunsByUKPRNUri = settings.Value.ToArchiveCourseRunsByUKPRNUri();
            _deleteBulkUploadCoursesUri = settings.Value.ToDeleteBulkUploadCoursesUri();
            _getCourseMigrationReportByUKPRN = settings.Value.ToGetCourseMigrationReportByUKPRN();
            _getAllDfcReports = settings.Value.ToGetAllDfcReports();
            _getTotalLiveCoursesUri = settings.Value.ToGetTotalLiveCourses();

            _courseForTextFieldMaxChars = courseForComponentSettings.Value.TextFieldMaxChars;
            _entryRequirementsTextFieldMaxChars = entryRequirementsComponentSettings.Value.TextFieldMaxChars;
            _whatWillLearnTextFieldMaxChars = whatWillLearnComponentSettings.Value.TextFieldMaxChars;
            _howYouWillLearnTextFieldMaxChars = howYouWillLearnComponentSettings.Value.TextFieldMaxChars;
            _whatYouNeedTextFieldMaxChars = whatYouNeedComponentSettings.Value.TextFieldMaxChars;
            _howAssessedTextFieldMaxChars = howAssessedComponentSettings.Value.TextFieldMaxChars;
            _whereNextTextFieldMaxChars = whereNextComponentSettings.Value.TextFieldMaxChars;

            _apiUserName = facSettings.Value.UserName;
            _apiPassword = facSettings.Value.Password;
        }

        public SelectRegionModel GetRegions()
        {
            var selectRegion = new SelectRegionModel
            {
                LabelText = "Where do you deliver this course?",
                HintText = "Choose all of the areas in as many regions as you deliver this course",
                AriaDescribedBy = "Select all that apply."
            };

            if (selectRegion.RegionItems != null && selectRegion.RegionItems.Any())
            {
                selectRegion.RegionItems = selectRegion.RegionItems.OrderBy(x => x.RegionName);
                foreach (var selectRegionRegionItem in selectRegion.RegionItems)
                {
                    selectRegionRegionItem.SubRegion = selectRegionRegionItem.SubRegion.OrderBy(x => x.SubRegionName).ToList();
                }
            }

            return selectRegion;
        }

        public async Task<IResult<ICourse>> GetCourseByIdAsync(IGetCourseByIdCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Get Course By Id criteria.", criteria);
                _logger.LogInformationObject("Get Course By Id URI", _getCourseByIdUri);

                var content = new StringContent(criteria.ToJson(), Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.GetAsync(new Uri(_getCourseByIdUri.AbsoluteUri + "?id=" + criteria.Id));

                _logger.LogHttpResponseMessage("Get Course By Id service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Get Course By Id service json response", json);

                    var course = JsonConvert.DeserializeObject<Course>(json);

                    return Result.Ok<ICourse>(course);
                }
                else
                {
                    return Result.Fail<ICourse>("Get Course By Id service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Get Course By Id service http request error", hre);
                return Result.Fail<ICourse>("Get Course By Id service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Get Course By Id service unknown error.", e);

                return Result.Fail<ICourse>("Get Course By Id service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }

        // TODO - Provider search is in the course service for now, needs moving!
        public async Task<IResult<ProviderAzureSearchResults>> ProviderSearchAsync(ProviderSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Provider search criteria", criteria);
                _logger.LogInformationObject("Provider search URI", _providerSearchUri);

                if (!_httpClient.DefaultRequestHeaders.Any(h => h.Key == "UserName"))
                    _httpClient.DefaultRequestHeaders.Add("UserName", _apiUserName);
                if (!_httpClient.DefaultRequestHeaders.Any(h => h.Key == "Password"))
                    _httpClient.DefaultRequestHeaders.Add("Password", _apiPassword);

                StringContent content = new StringContent(JsonConvert.SerializeObject(criteria),
                                                          Encoding.UTF8,
                                                          "application/json");

                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _facSettings.ApiKey);
                var response = await _httpClient.PostAsync(_providerSearchUri, content);

                _logger.LogHttpResponseMessage("Provider search service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                        return Result.Ok<ProviderAzureSearchResults>(new ProviderAzureSearchResults());

                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Provider search service json response", json);
                    ProviderAzureSearchResults providers = JsonConvert.DeserializeObject<ProviderAzureSearchResults>(json);

                    //ProviderSearchResult searchResult = new ProviderSearchResult(providers);
                    return Result.Ok<ProviderAzureSearchResults>(providers); // searchResult);
                }
                else
                    return Result.Fail<ProviderAzureSearchResults>("Provider search service unsuccessful http response");
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Provider search service http request error", hre);
                return Result.Fail<ProviderAzureSearchResults>("Provider search service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Provider search service unknown error.", e);
                return Result.Fail<ProviderAzureSearchResults>("Provider search service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }

        public async Task<IResult<ICourseSearchResult>> GetYourCoursesByUKPRNAsync(ICourseSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            Throw.IfLessThan(0, criteria.UKPRN.Value, nameof(criteria.UKPRN.Value));
            _logger.LogMethodEnter();
            HttpResponseMessage response = null;

            try
            {
                _logger.LogInformationObject("Get your courses criteria", criteria);
                _logger.LogInformationObject("Get your courses URI", _getYourCoursesUri);

                if (!criteria.UKPRN.HasValue)
                    return Result.Fail<ICourseSearchResult>("Get your courses unknown UKRLP");

                // use local version of httpclient as when we are called from the background worker thread we get socket exceptions
                HttpClient httpClient = new HttpClient();
                httpClient.Timeout = new TimeSpan(0, 10, 0);
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                response = await httpClient.GetAsync(new Uri(_getYourCoursesUri.AbsoluteUri + "?UKPRN=" + criteria.UKPRN));
                _logger.LogHttpResponseMessage("Get your courses service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!json.StartsWith("["))
                        json = "[" + json + "]";

                    _logger.LogInformationObject("Get your courses service json response", json);
                    IEnumerable<IEnumerable<IEnumerable<Course>>> courses = JsonConvert.DeserializeObject<IEnumerable<IEnumerable<IEnumerable<Course>>>>(json);

                    CourseSearchResult searchResult = new CourseSearchResult(courses);
                    return Result.Ok<ICourseSearchResult>(searchResult);
                }
                else
                {
                    return Result.Fail<ICourseSearchResult>("Get your courses service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Get your courses service http request error", hre);
                return Result.Fail<ICourseSearchResult>("Get your courses service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Get your courses service unknown error.", e);
                return Result.Fail<ICourseSearchResult>($"Get your courses service unknown error. {e.Message}");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }

        public async Task<IResult<ICourseSearchResult>> GetCoursesByLevelForUKPRNAsync(ICourseSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            Throw.IfLessThan(0, criteria.UKPRN.Value, nameof(criteria.UKPRN.Value));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Get your courses criteria", criteria);
                _logger.LogInformationObject("Get your courses URI", _getYourCoursesUri);

                if (!criteria.UKPRN.HasValue)
                    return Result.Fail<ICourseSearchResult>("Get your courses unknown UKRLP");

                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.GetAsync(new Uri(_getYourCoursesUri.AbsoluteUri + "?UKPRN=" + criteria.UKPRN));

                _logger.LogHttpResponseMessage("Get your courses service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!json.StartsWith("["))
                        json = "[" + json + "]";

                    _logger.LogInformationObject("Get your courses service json response", json);
                    IEnumerable<IEnumerable<IEnumerable<Course>>> courses = JsonConvert.DeserializeObject<IEnumerable<IEnumerable<IEnumerable<Course>>>>(json);
                    var searchResult = new CourseSearchResult(courses);

                    return Result.Ok<ICourseSearchResult>(searchResult);
                }
                else
                {
                    return Result.Fail<ICourseSearchResult>("Get your courses service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Get your courses service http request error", hre);
                return Result.Fail<ICourseSearchResult>("Get your courses service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Get your courses service unknown error.", e);
                return Result.Fail<ICourseSearchResult>("Get your courses service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }

        public async Task<IResult<IEnumerable<ICourseStatusCountResult>>> GetCourseCountsByStatusForUKPRN(ICourseSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            Throw.IfLessThan(0, criteria.UKPRN.Value, nameof(criteria.UKPRN.Value));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Get course counts criteria", criteria);
                _logger.LogInformationObject("Get course counts URI", _getCourseCountsByStatusForUKPRNUri);

                if (!criteria.UKPRN.HasValue)
                    return Result.Fail<IEnumerable<ICourseStatusCountResult>>("Get course counts unknown UKRLP");

                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.GetAsync(new Uri(_getCourseCountsByStatusForUKPRNUri.AbsoluteUri + "?UKPRN=" + criteria.UKPRN));

                _logger.LogHttpResponseMessage("Get course counts service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!json.StartsWith("["))
                        json = "[" + json + "]";

                    _logger.LogInformationObject("Get course counts service json response", json);
                    IEnumerable<ICourseStatusCountResult> counts = JsonConvert.DeserializeObject<IEnumerable<CourseStatusCountResult>>(json);

                    return Result.Ok<IEnumerable<ICourseStatusCountResult>>(counts);
                }
                else
                {
                    return Result.Fail<IEnumerable<ICourseStatusCountResult>>("Get course counts service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Get course counts service http request error", hre);
                return Result.Fail<IEnumerable<ICourseStatusCountResult>>("Get course counts service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Get course counts service unknown error.", e);
                return Result.Fail<IEnumerable<ICourseStatusCountResult>>("Get course counts service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }

        public async Task<IResult<IEnumerable<ICourse>>> GetRecentCourseChangesByUKPRN(ICourseSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            Throw.IfLessThan(0, criteria.UKPRN.Value, nameof(criteria.UKPRN.Value));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Get recent course changes criteria", criteria);
                _logger.LogInformationObject("Get recent course changes URI", _getRecentCourseChangesByUKPRNUri);

                if (!criteria.UKPRN.HasValue)
                    return Result.Fail<IEnumerable<ICourse>>("Get recent course changes unknown UKRLP");

                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.GetAsync(new Uri(_getRecentCourseChangesByUKPRNUri.AbsoluteUri + "?UKPRN=" + criteria.UKPRN));

                _logger.LogHttpResponseMessage("Get recent course changes service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!json.StartsWith("["))
                        json = "[" + json + "]";

                    _logger.LogInformationObject("Get recent course changes service json response", json);
                    IEnumerable<ICourse> courses = JsonConvert.DeserializeObject<IEnumerable<Course>>(json);

                    return Result.Ok<IEnumerable<ICourse>>(courses);
                }
                else
                {
                    return Result.Fail<IEnumerable<ICourse>>("Get recent course changes service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Get recent course changes service http request error", hre);
                return Result.Fail<IEnumerable<ICourse>>("Get recent course changes service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Get recent course changes service unknown error.", e);
                return Result.Fail<IEnumerable<ICourse>>("Get recent course changes service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }

        public IResult<IList<CourseValidationResult>> CourseValidationMessages(IEnumerable<ICourse> courses, ValidationMode mode)
        {
            _logger.LogMethodEnter();
            Throw.IfNull(courses, nameof(courses));

            try
            {
                IList<CourseValidationResult> results = new List<CourseValidationResult>();

                foreach (ICourse c in courses)
                {
                    CourseValidationResult cvr = new CourseValidationResult()
                    {
                        Course = c,
                        RunValidationResults = new List<CourseRunValidationResult>()
                    };
                    //Code to be refactored upon updated DQI stories

                    if (mode != ValidationMode.DataQualityIndicator)
                    {
                        cvr.Issues = ValidateCourse(c).Select(x => x.Value).ToList();
                    }
                    else
                    {
                        cvr.Issues = new List<string>();
                    }
                    foreach (ICourseRun r in c.CourseRuns)
                        cvr.RunValidationResults.Add(new CourseRunValidationResult() { Run = r, Issues = ValidateCourseRun(r, mode).Select(x => x.Value) });
                    results.Add(cvr);
                }
                return Result.Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogException("PendingCourseValidationMessages error", ex);
                return Result.Fail<IList<CourseValidationResult>>("Error compiling messages for items requiring attention on landing page");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }

        public async Task<IResult<ICourse>> AddCourseAsync(ICourse course)
        {
            _logger.LogMethodEnter();
            Throw.IfNull(course, nameof(course));

            try
            {
                _logger.LogInformationObject("Course add object.", course);
                _logger.LogInformationObject("Course add URI", _addCourseUri);

                var courseJson = JsonConvert.SerializeObject(course);

                var content = new StringContent(courseJson, Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.PostAsync(_addCourseUri, content);

                _logger.LogHttpResponseMessage("Course add service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Course add service json response", json);

                    var courseResult = JsonConvert.DeserializeObject<Course>(json);

                    return Result.Ok<ICourse>(courseResult);
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return Result.Fail<ICourse>("Course add service unsuccessful http response - TooManyRequests");
                }
                else
                {
                    return Result.Fail<ICourse>("Course add service unsuccessful http response - ResponseStatusCode: " + response.StatusCode);
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Course add service http request error", hre);
                return Result.Fail<ICourse>("Course add service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Course add service unknown error.", e);

                return Result.Fail<ICourse>("Course add service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }

        public async Task<IResult<ICourse>> UpdateCourseAsync(ICourse course)
        {
            _logger.LogMethodEnter();
            Throw.IfNull(course, nameof(course));

            try
            {
                _logger.LogInformationObject("Course update object.", course);
                _logger.LogInformationObject("Course update URI", _updateCourseUri);

                var courseJson = JsonConvert.SerializeObject(course);

                var content = new StringContent(courseJson, Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.PostAsync(_updateCourseUri, content);

                _logger.LogHttpResponseMessage("Course update service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Course update service json response", json);

                    var courseResult = JsonConvert.DeserializeObject<Course>(json);

                    return Result.Ok<ICourse>(courseResult);
                }
                else
                {
                    return Result.Fail<ICourse>("Course update service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Course update service http request error", hre);
                return Result.Fail<ICourse>("Course update service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Course update service unknown error.", e);

                return Result.Fail<ICourse>("Course update service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }

        public IList<KeyValuePair<string, string>> ValidateCourse(ICourse course)
        {
            List<KeyValuePair<string, string>> validationMessages = new List<KeyValuePair<string, string>>();

            // CourseDescription
            if (string.IsNullOrEmpty(course.CourseDescription))
            {
                validationMessages.Add(new KeyValuePair<string, string>("WHO_IS_THIS_COURSE_FOR", "Course For description is required"));
            }
            else
            {
                if (!HasOnlyFollowingValidCharacters(course.CourseDescription))
                    validationMessages.Add(new KeyValuePair<string, string>("WHO_IS_THIS_COURSE_FOR", "Course For description contains invalid character"));
                if (course.CourseDescription.Length > _courseForTextFieldMaxChars)
                    validationMessages.Add(new KeyValuePair<string, string>("WHO_IS_THIS_COURSE_FOR", $"Who is this course for? must be { _courseForTextFieldMaxChars } characters or less"));
            }

            // EntryRequirements
            if (!string.IsNullOrEmpty(course.EntryRequirements))
            {
                if (!HasOnlyFollowingValidCharacters(course.EntryRequirements))
                    validationMessages.Add(new KeyValuePair<string, string>("ENTRY_REQUIREMENTS", "Entry Requirements contains invalid character"));
                if (course.EntryRequirements.Length > _entryRequirementsTextFieldMaxChars)
                    validationMessages.Add(new KeyValuePair<string, string>("ENTRY_REQUIREMENTS", $"Entry Requirements must be { _entryRequirementsTextFieldMaxChars } characters or less"));
            }

            // WhatYoullLearn
            if (!string.IsNullOrEmpty(course.WhatYoullLearn))
            {
                if (!HasOnlyFollowingValidCharacters(course.WhatYoullLearn))
                    validationMessages.Add(new KeyValuePair<string, string>("WHAT_YOU_WILL_LEARN", "What you will Learn contains invalid character"));
                if (course.WhatYoullLearn.Length > _whatWillLearnTextFieldMaxChars)
                    validationMessages.Add(new KeyValuePair<string, string>("WHAT_YOU_WILL_LEARN", $"What you will Learn must be { _whatWillLearnTextFieldMaxChars } characters or less"));
            }

            // HowYoullLearn
            if (!string.IsNullOrEmpty(course.HowYoullLearn))
            {
                if (!HasOnlyFollowingValidCharacters(course.HowYoullLearn))
                    validationMessages.Add(new KeyValuePair<string, string>("HOW_YOU_WILL_LEARN", "How you'll learn contains invalid character"));
                if (course.HowYoullLearn.Length > _howYouWillLearnTextFieldMaxChars)
                    validationMessages.Add(new KeyValuePair<string, string>("HOW_YOU_WILL_LEARN", $"How you'll learn must be { _howYouWillLearnTextFieldMaxChars } characters or less"));
            }

            // WhatYoullNeed
            if (!string.IsNullOrEmpty(course.WhatYoullNeed))
            {
                if (!HasOnlyFollowingValidCharacters(course.WhatYoullNeed))
                    validationMessages.Add(new KeyValuePair<string, string>("WHAT_YOU_WILL_NEED_TO_BRING", "What you'll need to bring contains invalid character"));
                if (course.WhatYoullNeed.Length > _whatYouNeedTextFieldMaxChars)
                    validationMessages.Add(new KeyValuePair<string, string>("WHAT_YOU_WILL_NEED_TO_BRING", $"What you'll need to bring must be { _whatYouNeedTextFieldMaxChars } characters or less"));
            }

            // HowYoullBeAssessed
            if (!string.IsNullOrEmpty(course.HowYoullBeAssessed))
            {
                if (!HasOnlyFollowingValidCharacters(course.HowYoullBeAssessed))
                    validationMessages.Add(new KeyValuePair<string, string>("HOW_YOU_WILL_BE_ASSESSED", "How you'll be assessed contains invalid character"));
                if (course.HowYoullBeAssessed.Length > _howAssessedTextFieldMaxChars)
                    validationMessages.Add(new KeyValuePair<string, string>("HOW_YOU_WILL_BE_ASSESSED", $"How you'll be assessed must be { _howAssessedTextFieldMaxChars } characters or less"));
            }

            // WhereNext
            if (!string.IsNullOrEmpty(course.WhereNext))
            {
                if (!HasOnlyFollowingValidCharacters(course.WhereNext))
                    validationMessages.Add(new KeyValuePair<string, string>("WHERE_NEXT", "'Where next' contains invalid character"));
                if (course.WhereNext.Length > _whereNextTextFieldMaxChars)
                    validationMessages.Add(new KeyValuePair<string, string>("WHERE_NEXT", $"'Where next' must be { _whereNextTextFieldMaxChars } characters or less"));
            }

            return validationMessages;
        }

        public IList<KeyValuePair<string, string>> ValidateCourseRun(ICourseRun courseRun, ValidationMode validationMode)
        {
            IList<KeyValuePair<string, string>> validationMessages = new List<KeyValuePair<string, string>>();

            //Filtered down validation rules for DQI based on story
            //To be made more generic when we bring additional rules in
            if (validationMode == ValidationMode.DataQualityIndicator)
            {
                if (courseRun.StartDate < DateTime.Today)
                    validationMessages.Add(new KeyValuePair<string, string>("START_DATE", $"courses need their start date updating"));
                return validationMessages;
            }

            // CourseName
            if (string.IsNullOrEmpty(courseRun.CourseName))
            {
                validationMessages.Add(new KeyValuePair<string, string>("COURSE_NAME", "Enter course name"));
            }
            else
            {
                if (!HasOnlyFollowingValidCharacters(courseRun.CourseName))
                    validationMessages.Add(new KeyValuePair<string, string>("COURSE_NAME", "Course Name contains invalid character"));
                if (courseRun.CourseName.Length > 255)
                    validationMessages.Add(new KeyValuePair<string, string>("COURSE_NAME", $"Course Name must be 255 characters or less"));
            }

            // ProviderCourseID
            if (!string.IsNullOrEmpty(courseRun.ProviderCourseID))
            {
                if (!HasOnlyFollowingValidCharacters(courseRun.ProviderCourseID))
                    validationMessages.Add(new KeyValuePair<string, string>("ID", "ID contains invalid characters"));
                if (courseRun.ProviderCourseID.Length > 255)
                    validationMessages.Add(new KeyValuePair<string, string>("ID", $"The maximum length of 'ID' is 255 characters"));
            }

            // DeliveryMode
            switch (courseRun.DeliveryMode)
            {
                case DeliveryMode.ClassroomBased:

                    // VenueId
                    if (courseRun.VenueId == null || courseRun.VenueId == Guid.Empty)
                        validationMessages.Add(new KeyValuePair<string, string>("VENUE", $"Select venue"));

                    // StudyMode
                    if (courseRun.StudyMode.Equals(StudyMode.Undefined))
                        validationMessages.Add(new KeyValuePair<string, string>("STUDY_MODE", $"Select Study Mode"));

                    // AttendancePattern
                    if (courseRun.AttendancePattern.Equals(AttendancePattern.Undefined))
                        validationMessages.Add(new KeyValuePair<string, string>("ATTENDANCE_PATTERN", $"Select Attendance Mode"));

                    break;

                case DeliveryMode.Online:
                    // No Specific Fields
                    break;

                case DeliveryMode.WorkBased:

                    //National
                    if (courseRun.National == null)
                    {
                        validationMessages.Add(new KeyValuePair<string, string>("NATIONAL_DELIVERY", $"Choose if you can deliver this course anywhere in England"));
                    }
                    else if (courseRun.National == false)
                    {
                        // Regions
                        if (courseRun.Regions == null || courseRun.Regions.Count().Equals(0))
                            validationMessages.Add(new KeyValuePair<string, string>("REGION", $"Select at least one region or sub-region"));
                    }
                    break;

                case DeliveryMode.Undefined: // Question ???
                default:
                    validationMessages.Add(new KeyValuePair<string, string>("DELIVERY_MODE", $"Select Delivery Mode"));
                    break;
            }

            // StartDate & FlexibleStartDate
            if (courseRun.StartDate != null)
            {
                courseRun.FlexibleStartDate = false; // COUR-746-StartDate

                var currentDate = DateTime.UtcNow.Date;

                switch (validationMode)
                {
                    case ValidationMode.AddCourseRun:
                    case ValidationMode.CopyCourseRun:
                    case ValidationMode.EditCourseBU:
                    case ValidationMode.BulkUploadCourse:

                        _logger.LogError("course date" + courseRun.StartDate.Value.Date + "utc Date " + currentDate);

                        int result = DateTime.Compare(courseRun.StartDate.Value.Date, currentDate);

                        if (result < 0)
                        {
                            _logger.LogWarning("*Simon* Date in the past");
                        }

                        if (courseRun.StartDate < currentDate)
                            validationMessages.Add(new KeyValuePair<string, string>("START_DATE", $"Start Date cannot be earlier than today's date"));
                        if (courseRun.StartDate > currentDate.AddYears(2))
                            validationMessages.Add(new KeyValuePair<string, string>("START_DATE", $"Start Date cannot be later than 2 years from today’s date"));
                        break;

                    case ValidationMode.EditCourseYC:
                    case ValidationMode.EditCourseMT:
                        // It cannot be done easily as we need both value - the newly entered and the previous. Call to saved version or modification in the model
                        break;

                    case ValidationMode.MigrateCourse:
                        if (courseRun.StartDate > currentDate.AddYears(2))
                            validationMessages.Add(new KeyValuePair<string, string>("START_DATE", $"Start Date cannot be later than 2 years from today’s date"));
                        break;

                    case ValidationMode.Undefined:
                    default:
                        validationMessages.Add(new KeyValuePair<string, string>("START_DATE", $"Validation Mode was not defined."));
                        break;
                }
            }

            if (courseRun.StartDate == null && courseRun.FlexibleStartDate == false)
                validationMessages.Add(new KeyValuePair<string, string>("START_DATE+FLEXIBLE_START_DATE", $"Either 'Defined Start Date' or 'Flexible Start Date' has to be provided"));

            // CourseURL
            if (!string.IsNullOrEmpty(courseRun.CourseURL))
            {
                if (!IsValidUrl(courseRun.CourseURL))
                    validationMessages.Add(new KeyValuePair<string, string>("URL", "The format of URL is incorrect"));
                if (courseRun.CourseURL.Length > 255)
                    validationMessages.Add(new KeyValuePair<string, string>("URL", $"The maximum length of URL is 255 characters"));
            }

            // Cost & CostDescription
            if (string.IsNullOrEmpty(courseRun.CostDescription) && courseRun.Cost.Equals(null))
                validationMessages.Add(new KeyValuePair<string, string>("COST", $"Enter cost or cost description"));

            if (!string.IsNullOrEmpty(SpecialCharacters(courseRun.CostDescription)))
            {
                
                if (!HasOnlyFollowingValidCharacters(SpecialCharacters(courseRun.CostDescription)))
                    validationMessages.Add(new KeyValuePair<string, string>("COST_DESCRIPTION", "Cost Description contains invalid characters"));
                if (courseRun.CostDescription.Length > 255)
                    validationMessages.Add(new KeyValuePair<string, string>("COST_DESCRIPTION", $"Cost description must be 255 characters or less"));
            }

            if (!courseRun.Cost.Equals(null))
            {
                if (!IsCorrectCostFormatting(courseRun.Cost.ToString()))
                    validationMessages.Add(new KeyValuePair<string, string>("COST", $"Enter the cost in pounds and pence"));
                if (courseRun.Cost > decimal.Parse("999999.99"))
                    validationMessages.Add(new KeyValuePair<string, string>("COST", $"Maximum allowed cost value is 999,999.99"));
            }

            // DurationValue and DurationUnit
            if (courseRun.DurationValue.Equals(null) || courseRun.DurationUnit.Equals(DurationUnit.Undefined))
            {
                validationMessages.Add(new KeyValuePair<string, string>("DURATION", $"Enter duration"));
            }
            else
            {
                if (!ValidDurationValue(courseRun.DurationValue?.ToString()))
                    validationMessages.Add(new KeyValuePair<string, string>("DURATION", "Duration must be numeric and maximum length is 3 digits"));
            }

            return validationMessages;
        }

        public bool HasOnlyFollowingValidCharacters(string value)
        {
            string regex = @"^[a-zA-Z0-9 /\n/\r/\\u/\¬\!\£\$\%\^\&\*\\é\\è\\ﬁ\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`\•\·\●\\’\‘\“\”\—\-\–\‐\‐\…\:/\°\®\\â\\ç\\ñ\\ü\\ø\♦\™\\t/\s\¼\¾\½\" + "\"" + "\\\\]+$";
            var validUKPRN = Regex.Match(value, regex, RegexOptions.IgnoreCase);

            return validUKPRN.Success;
        }

        public bool IsValidUrl(string value)
        {
            string regex = @"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$";
            var validUKPRN = Regex.Match(value, regex, RegexOptions.IgnoreCase);

            return validUKPRN.Success;
        }

        public bool IsCorrectCostFormatting(string value)
        {
            string regex = @"^[0-9]*(\.[0-9]{1,2})?$";
            var validUKPRN = Regex.Match(value, regex, RegexOptions.IgnoreCase);

            return validUKPRN.Success;
        }

        public bool ValidDurationValue(string value)
        {
            string regex = @"^([0-9]|[0-9][0-9]|[0-9][0-9][0-9])$";
            var validUKPRN = Regex.Match(value, regex, RegexOptions.IgnoreCase);

            return validUKPRN.Success;
        }

        public string SpecialCharacters(string value)
        {
            var replacedString = value.Replace("â€™", "'").Replace("â€“", "–").Replace("�", "£");
            return replacedString;
        }

        public async Task<IResult> ArchiveProviderLiveCourses(int? UKPRN)
        {
            Throw.IfNull(UKPRN, nameof(UKPRN));

            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
            var response = await _httpClient.GetAsync(new Uri(_archiveLiveCoursesUri.AbsoluteUri + "?UKPRN=" + UKPRN));
            _logger.LogHttpResponseMessage("Archive courses service http response", response);

            if (response.IsSuccessStatusCode)
            {
                return Result.Ok();
            }
            else
            {
                return Result.Fail("Archive courses service unsuccessful http response");
            }
        }

        public async Task<IResult> ChangeCourseRunStatusesForUKPRNSelection(int UKPRN, int CurrentStatus, int StatusToBeChangedTo)
        {
            Throw.IfNull(UKPRN, nameof(UKPRN));
            Throw.IfNull(CurrentStatus, nameof(CurrentStatus));
            Throw.IfNull(StatusToBeChangedTo, nameof(StatusToBeChangedTo));

            // @ToDo: sort out this ugly hack that fixes the TaskCancelledException when this is called from the background worker in the CourseDirectory app per
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);

            //_httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
            var response = await httpClient.GetAsync(new Uri(_changeCourseRunStatusesForUKPRNSelectionUri.AbsoluteUri + "?UKPRN=" + UKPRN + "&CurrentStatus=" + CurrentStatus + "&StatusToBeChangedTo=" + StatusToBeChangedTo));
            _logger.LogHttpResponseMessage("Archive courses service http response", response);

            if (response.IsSuccessStatusCode)
            {
                return Result.Ok();
            }
            else
            {
                return Result.Fail("ChangeCourseRunStatusesForUKPRNSelection service unsuccessful http response");
            }
        }

        public async Task<IResult> ArchiveCourseRunsByUKPRN(int UKPRN)
        {
            Throw.IfNull(UKPRN, nameof(UKPRN));

            // @ToDo: sort out this ugly hack that fixes the TaskCancelledException when this is called from the background worker in the CourseDirectory app per
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);

            //_httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
            var response = await httpClient.GetAsync(new Uri(_archiveCourseRunsByUKPRNUri.AbsoluteUri + "?UKPRN=" + UKPRN));
            _logger.LogHttpResponseMessage("Archive courses service http response", response);

            if (response.IsSuccessStatusCode)
            {
                return Result.Ok();
            }
            else
            {
                return Result.Fail("ChangeCourseRunStatusesForUKPRNSelection service unsuccessful http response");
            }
        }

        public async Task<IResult> UpdateStatus(Guid courseId, Guid courseRunId, int statusToUpdateTo)
        {
            Throw.IfNullGuid(courseId, nameof(courseId));
            Throw.IfLessThan(0, statusToUpdateTo, nameof(statusToUpdateTo));
            Throw.IfGreaterThan(Enum.GetValues(typeof(RecordStatus)).Cast<int>().Max(), statusToUpdateTo, nameof(statusToUpdateTo));

            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
            var response = await _httpClient.GetAsync(new Uri(_updateStatusUri.AbsoluteUri
                + "?CourseId=" + courseId
                + "&CourseRunId=" + courseRunId
                + "&Status=" + statusToUpdateTo));
            _logger.LogHttpResponseMessage("Update Status http response", response);

            if (response.IsSuccessStatusCode)
            {
                return Result.Ok();
            }
            else
            {
                return Result.Fail("Update course unsuccessful http response");
            }
        }

        public async Task<IResult> DeleteBulkUploadCourses(int UKPRN)
        {
            Throw.IfLessThan(0, UKPRN, nameof(UKPRN));

            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await httpClient.GetAsync(new Uri(_deleteBulkUploadCoursesUri.AbsoluteUri
                    + "?UKPRN=" + UKPRN));
                _logger.LogHttpResponseMessage("Delete Bulk Upload Course Status http response", response);

                if (response.IsSuccessStatusCode)
                {
                    return Result.Ok();
                }
                else
                {
                    return Result.Fail("Delete Bulk Upload Course unsuccessful: " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                return Result.Fail("Update course unsuccessful http response");
            }
        }

        public async Task<IResult<CourseMigrationReport>> GetCourseMigrationReport(int UKPRN)
        {
            Throw.IfNull(UKPRN, nameof(UKPRN));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Get your courses URI", _getYourCoursesUri);

                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.GetAsync(new Uri(_getCourseMigrationReportByUKPRN.AbsoluteUri + "?UKPRN=" + UKPRN));
                _logger.LogHttpResponseMessage("Get course migration report service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Get course migration report service json response", json);
                    CourseMigrationReport courseMigrationReport = JsonConvert.DeserializeObject<CourseMigrationReport>(json);
                    return Result.Ok<CourseMigrationReport>(courseMigrationReport);
                }
                else
                {
                    return Result.Fail<CourseMigrationReport>("Get course migration report service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Get course migration report service http request error", hre);
                return Result.Fail<CourseMigrationReport>("Get course migration report service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Get course migration report service unknown error.", e);
                return Result.Fail<CourseMigrationReport>("Get course migration report service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }

        public async Task<IResult<IList<DfcMigrationReport>>> GetAllDfcReports()
        {
            _logger.LogMethodEnter();

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
            finally
            {
                _logger.LogMethodExit();
            }
        }

        public async Task<IResult<int>> GetTotalLiveCourses()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);

                var response = await httpClient.GetAsync(_getTotalLiveCoursesUri);
                _logger.LogHttpResponseMessage("GetTotalLiveCourses service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var total = JsonConvert.DeserializeObject<int>(json);

                    return Result.Ok(total);
                }
                else
                {
                    return Result.Fail<int>("GetTotalLiveCourses service unsuccessful http response");
                }
            }
        }
    }

    internal static class IGetCourseByIdCriteriaExtensions
    {
        internal static string ToJson(this IGetCourseByIdCriteria extendee)
        {
            GetCourseByIdJson json = new GetCourseByIdJson
            {
                id = extendee.Id.ToString()
            };
            var result = JsonConvert.SerializeObject(json);

            return result;
        }
    }

    internal class GetCourseByIdJson
    {
        public string id { get; set; }
    }

    internal static class CourseServiceSettingsExtensions
    {
        internal static Uri ToAddCourseUri(this ICourseServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/AddCourse");
        }

        internal static Uri ToGetYourCoursesUri(this ICourseServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetCoursesByLevelForUKPRN");
        }

        internal static Uri ToUpdateCourseUri(this ICourseServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/UpdateCourse");
        }

        internal static Uri ToGetCourseByIdUri(this ICourseServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetCourseById");
        }

        internal static Uri ToArchiveLiveCoursesUri(this ICourseServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/ArchiveProvidersLiveCourses");
        }

        internal static Uri ToUpdateStatusUri(this ICourseServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/UpdateStatus");
        }

        internal static Uri ToGetCourseCountsByStatusForUKPRNUri(this ICourseServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetCourseCountsByStatusForUKPRN");
        }

        internal static Uri ToGetRecentCourseChangesByUKPRNUri(this ICourseServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetRecentCourseChangesByUKPRN");
        }

        internal static Uri ToChangeCourseRunStatusesForUKPRNSelectionUri(this ICourseServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/ChangeCourseRunStatusesForUKPRNSelection");
        }

        internal static Uri ToArchiveCourseRunsByUKPRNUri(this ICourseServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/ArchiveCourseRunsByUKPRN");
        }

        internal static Uri ToDeleteBulkUploadCoursesUri(this ICourseServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/DeleteBulkUploadCourses");
        }

        internal static Uri ToGetCourseMigrationReportByUKPRN(this ICourseServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetCourseMigrationReportByUKPRN");
        }

        internal static Uri ToGetAllDfcReports(this ICourseServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetAllDfcReports");
        }

        internal static Uri ToGetTotalLiveCourses(this ICourseServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetTotalLiveCourses");
        }
    }

    internal static class FindACourseServiceSettingsExtensions
    {
        internal static Uri ToFindACourseSearchUri(this IFindACourseServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/search");
        }

        internal static Uri ToProviderSearchUri(this IFindACourseServiceSettings extendee)
        {
            var uri = new Uri(extendee.ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/providersearch");
        }
    }
}