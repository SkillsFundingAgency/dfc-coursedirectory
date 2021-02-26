using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Services.Models.Regions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
        private readonly Uri _updateCourseUri;
        private readonly Uri _getCourseByIdUri;
        private readonly Uri _updateStatusUri;
        private readonly Uri _getCourseCountsByStatusForUKPRNUri;
        private readonly Uri _changeCourseRunStatusesForUKPRNSelectionUri;
        private readonly Uri _archiveCourseRunsByUKPRNUri;
        private readonly Uri _deleteBulkUploadCoursesUri;
        private readonly Uri _getCourseMigrationReportByUKPRN;
        private readonly Uri _archiveCoursesExceptBulkUploadReadytoGoLiveUri;

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

            if (facSettings == null)
            {
                throw new ArgumentNullException(nameof(facSettings));
            }

            if (courseForComponentSettings == null)
            {
                throw new ArgumentNullException(nameof(courseForComponentSettings));
            }

            if (entryRequirementsComponentSettings == null)
            {
                throw new ArgumentNullException(nameof(entryRequirementsComponentSettings));
            }

            if (whatWillLearnComponentSettings == null)
            {
                throw new ArgumentNullException(nameof(whatWillLearnComponentSettings));
            }

            if (howYouWillLearnComponentSettings == null)
            {
                throw new ArgumentNullException(nameof(howYouWillLearnComponentSettings));
            }

            if (whatYouNeedComponentSettings == null)
            {
                throw new ArgumentNullException(nameof(whatYouNeedComponentSettings));
            }

            if (howAssessedComponentSettings == null)
            {
                throw new ArgumentNullException(nameof(howAssessedComponentSettings));
            }

            if (whereNextComponentSettings == null)
            {
                throw new ArgumentNullException(nameof(whereNextComponentSettings));
            }

            _logger = logger;
            _settings = settings.Value;
            _facSettings = facSettings.Value;
            _httpClient = httpClient;

            _addCourseUri = settings.Value.ToAddCourseUri();
            _getYourCoursesUri = settings.Value.ToGetYourCoursesUri();
            _updateCourseUri = settings.Value.ToUpdateCourseUri();
            _getCourseByIdUri = settings.Value.ToGetCourseByIdUri();
            _updateStatusUri = settings.Value.ToUpdateStatusUri();
            _getCourseCountsByStatusForUKPRNUri = settings.Value.ToGetCourseCountsByStatusForUKPRNUri();
            _changeCourseRunStatusesForUKPRNSelectionUri = settings.Value.ToChangeCourseRunStatusesForUKPRNSelectionUri();
            _archiveCourseRunsByUKPRNUri = settings.Value.ToArchiveCourseRunsByUKPRNUri();
            _deleteBulkUploadCoursesUri = settings.Value.ToDeleteBulkUploadCoursesUri();
            _getCourseMigrationReportByUKPRN = settings.Value.ToGetCourseMigrationReportByUKPRN();
            _archiveCoursesExceptBulkUploadReadytoGoLiveUri = settings.Value.ToArchiveCoursesExceptBulkUploadReadytoGoLiveUri();

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
                LabelText = "Where in England can you deliver this course?",
                HintText = "Select all regions and areas that apply.",
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

        public async Task<Result<Course>> GetCourseByIdAsync(GetCourseByIdCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            try
            {
                var content = new StringContent(ToJson(criteria), Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.GetAsync(new Uri(_getCourseByIdUri.AbsoluteUri + "?id=" + criteria.Id));

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var course = JsonConvert.DeserializeObject<Course>(json);

                    return Result.Ok(course);
                }
                else
                {
                    return Result.Fail<Course>("Get Course By Id service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Get Course By Id service http request error");
                return Result.Fail<Course>("Get Course By Id service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Get Course By Id service unknown error.");

                return Result.Fail<Course>("Get Course By Id service unknown error.");
            }
        }

        public async Task<Result<CourseSearchResult>> GetYourCoursesByUKPRNAsync(CourseSearchCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            if (criteria.UKPRN < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(criteria.UKPRN), $"{nameof(criteria.UKPRN)} cannot be less than 0.");
            }

            try
            {
                if (!criteria.UKPRN.HasValue)
                    return Result.Fail<CourseSearchResult>("Get your courses unknown UKRLP");

                // use local version of httpclient as when we are called from the background worker thread we get socket exceptions
                HttpClient httpClient = new HttpClient();
                httpClient.Timeout = new TimeSpan(0, 10, 0);
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await httpClient.GetAsync(new Uri(_getYourCoursesUri.AbsoluteUri + "?UKPRN=" + criteria.UKPRN));

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!json.StartsWith("["))
                        json = "[" + json + "]";

                    var courses = JsonConvert.DeserializeObject<IEnumerable<IEnumerable<IEnumerable<Course>>>>(json);

                    CourseSearchResult searchResult = new CourseSearchResult(courses);
                    return Result.Ok(searchResult);
                }
                else
                {
                    return Result.Fail<CourseSearchResult>("Get your courses service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Get your courses service http request error");
                return Result.Fail<CourseSearchResult>("Get your courses service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Get your courses service unknown error.");
                return Result.Fail<CourseSearchResult>($"Get your courses service unknown error. {e.Message}");
            }
        }

        public async Task<Result<CourseSearchResult>> GetCoursesByLevelForUKPRNAsync(CourseSearchCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            if (criteria.UKPRN < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(criteria.UKPRN), $"{nameof(criteria.UKPRN)} cannot be less than 0.");
            }

            try
            {
                if (!criteria.UKPRN.HasValue)
                    return Result.Fail<CourseSearchResult>("Get your courses unknown UKRLP");

                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.GetAsync(new Uri(_getYourCoursesUri.AbsoluteUri + "?UKPRN=" + criteria.UKPRN));

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!json.StartsWith("["))
                        json = "[" + json + "]";

                    var courses = JsonConvert.DeserializeObject<IEnumerable<IEnumerable<IEnumerable<Course>>>>(json);
                    var searchResult = new CourseSearchResult(courses);

                    return Result.Ok(searchResult);
                }
                else
                {
                    return Result.Fail<CourseSearchResult>("Get your courses service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Get your courses service http request error");
                return Result.Fail<CourseSearchResult>("Get your courses service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Get your courses service unknown error.");
                return Result.Fail<CourseSearchResult>("Get your courses service unknown error.");
            }
        }

        public async Task<Result<IEnumerable<CourseStatusCountResult>>> GetCourseCountsByStatusForUKPRN(CourseSearchCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            if (criteria.UKPRN < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(criteria.UKPRN), $"{nameof(criteria.UKPRN)} cannot be less than 0.");
            }

            try
            {
                if (!criteria.UKPRN.HasValue)
                {
                    return Result.Fail<IEnumerable<CourseStatusCountResult>>("Get course counts unknown UKRLP");
                }

                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.GetAsync(new Uri(_getCourseCountsByStatusForUKPRNUri.AbsoluteUri + "?UKPRN=" + criteria.UKPRN));

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!json.StartsWith("["))
                        json = "[" + json + "]";

                    var counts = JsonConvert.DeserializeObject<IEnumerable<CourseStatusCountResult>>(json);

                    return Result.Ok(counts);
                }
                else
                {
                    return Result.Fail<IEnumerable<CourseStatusCountResult>>("Get course counts service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Get course counts service http request error");
                return Result.Fail<IEnumerable<CourseStatusCountResult>>("Get course counts service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Get course counts service unknown error.");
                return Result.Fail<IEnumerable<CourseStatusCountResult>>("Get course counts service unknown error.");
            }
        }

        public Result<IList<CourseValidationResult>> CourseValidationMessages(IEnumerable<Course> courses, ValidationMode mode)
        {
            if (courses == null)
            {
                throw new ArgumentNullException(nameof(courses));
            }

            try
            {
                IList<CourseValidationResult> results = new List<CourseValidationResult>();

                foreach (Course c in courses)
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
                    foreach (CourseRun r in c.CourseRuns)
                        cvr.RunValidationResults.Add(new CourseRunValidationResult() { Run = r, Issues = ValidateCourseRun(r, mode).Select(x => x.Value) });
                    results.Add(cvr);
                }
                return Result.Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PendingCourseValidationMessages error");
                return Result.Fail<IList<CourseValidationResult>>("Error compiling messages for items requiring attention on landing page");
            }
        }

        public async Task<Result<Course>> AddCourseAsync(Course course)
        {
            if (course == null)
            {
                throw new ArgumentNullException(nameof(course));
            }

            try
            {
                var courseJson = JsonConvert.SerializeObject(course);

                var content = new StringContent(courseJson, Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.PostAsync(_addCourseUri, content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var courseResult = JsonConvert.DeserializeObject<Course>(json);

                    return Result.Ok(courseResult);
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return Result.Fail<Course>("Course add service unsuccessful http response - TooManyRequests");
                }
                else
                {
                    return Result.Fail<Course>("Course add service unsuccessful http response - ResponseStatusCode: " + response.StatusCode);
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Course add service http request error");
                return Result.Fail<Course>("Course add service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Course add service unknown error.");
                return Result.Fail<Course>("Course add service unknown error.");
            }
        }

        public async Task<Result<Course>> UpdateCourseAsync(Course course)
        {
            if (course == null)
            {
                throw new ArgumentNullException(nameof(course));
            }

            try
            {
                var courseJson = JsonConvert.SerializeObject(course);

                var content = new StringContent(courseJson, Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.PostAsync(_updateCourseUri, content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var courseResult = JsonConvert.DeserializeObject<Course>(json);

                    return Result.Ok(courseResult);
                }
                else
                {
                    return Result.Fail<Course>("Course update service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Course update service http request error");
                return Result.Fail<Course>("Course update service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Course update service unknown error.");

                return Result.Fail<Course>("Course update service unknown error.");
            }
        }

        public IList<KeyValuePair<string, string>> ValidateCourse(Course course)
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

        public IList<KeyValuePair<string, string>> ValidateCourseRun(CourseRun courseRun, ValidationMode validationMode)
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

            if (!string.IsNullOrEmpty(ReplaceSpecialCharacters(courseRun.CostDescription)))
            {
                
                if (!HasOnlyFollowingValidCharacters(ReplaceSpecialCharacters(courseRun.CostDescription)))
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

        public string ReplaceSpecialCharacters(string value)
        {
            if (value == null)
            {
                return null;
            }

            return value
                .Replace("â€™", "'")
                .Replace("â€“", "–")
                .Replace("�", "£");
        }

        public async Task<Result> ChangeCourseRunStatusesForUKPRNSelection(int UKPRN, int CurrentStatus, int StatusToBeChangedTo)
        {
            // @ToDo: sort out this ugly hack that fixes the TaskCancelledException when this is called from the background worker in the CourseDirectory app per
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);

            var response = await httpClient.GetAsync(new Uri(_changeCourseRunStatusesForUKPRNSelectionUri.AbsoluteUri + "?UKPRN=" + UKPRN + "&CurrentStatus=" + CurrentStatus + "&StatusToBeChangedTo=" + StatusToBeChangedTo));

            if (response.IsSuccessStatusCode)
            {
                return Result.Ok();
            }
            else
            {
                return Result.Fail("ChangeCourseRunStatusesForUKPRNSelection service unsuccessful http response");
            }
        }

        public async Task<Result> ArchiveCourseRunsByUKPRN(int UKPRN)
        {
            // @ToDo: sort out this ugly hack that fixes the TaskCancelledException when this is called from the background worker in the CourseDirectory app per
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);

            var response = await httpClient.GetAsync(new Uri(_archiveCourseRunsByUKPRNUri.AbsoluteUri + "?UKPRN=" + UKPRN));

            if (response.IsSuccessStatusCode)
            {
                return Result.Ok();
            }
            else
            {
                return Result.Fail("ChangeCourseRunStatusesForUKPRNSelection service unsuccessful http response");
            }
        }

        public async Task<Result> UpdateStatus(Guid courseId, Guid courseRunId, int statusToUpdateTo)
        {
            if (statusToUpdateTo < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(statusToUpdateTo), $"{nameof(statusToUpdateTo)} cannot be less than 0.");
            }

            var maxRecordStatus = Enum.GetValues(typeof(RecordStatus)).Cast<int>().Max();

            if (statusToUpdateTo > maxRecordStatus)
            {
                throw new ArgumentOutOfRangeException(nameof(statusToUpdateTo), $"{nameof(statusToUpdateTo)} cannot be greater than {nameof(maxRecordStatus)}.");
            }

            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);

            var response = await _httpClient.GetAsync(new Uri(_updateStatusUri.AbsoluteUri
                + "?CourseId=" + courseId
                + "&CourseRunId=" + courseRunId
                + "&Status=" + statusToUpdateTo));

            if (response.IsSuccessStatusCode)
            {
                return Result.Ok();
            }
            else
            {
                return Result.Fail("Update course unsuccessful http response");
            }
        }

        public async Task<Result> DeleteBulkUploadCourses(int UKPRN)
        {
            if (UKPRN < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(UKPRN), $"{nameof(UKPRN)} cannot be less than 0.");
            }

            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await httpClient.GetAsync(new Uri(_deleteBulkUploadCoursesUri.AbsoluteUri
                    + "?UKPRN=" + UKPRN));

                if (response.IsSuccessStatusCode)
                {
                    return Result.Ok();
                }
                else
                {
                    return Result.Fail("Delete Bulk Upload Course unsuccessful: " + response.ReasonPhrase);
                }
            }
            catch (Exception)
            {
                return Result.Fail("Update course unsuccessful http response");
            }
        }

        public async Task<Result<CourseMigrationReport>> GetCourseMigrationReport(int UKPRN)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.GetAsync(new Uri(_getCourseMigrationReportByUKPRN.AbsoluteUri + "?UKPRN=" + UKPRN));

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var courseMigrationReport = JsonConvert.DeserializeObject<CourseMigrationReport>(json);
                    return Result.Ok(courseMigrationReport);
                }
                else
                {
                    return Result.Fail<CourseMigrationReport>("Get course migration report service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Get course migration report service http request error");
                return Result.Fail<CourseMigrationReport>("Get course migration report service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Get course migration report service unknown error.");
                return Result.Fail<CourseMigrationReport>("Get course migration report service unknown error.");
            }
        }

        public async Task<Result> ArchiveCoursesExceptBulkUploadReadytoGoLive(int UKPRN,int StatusToBeChangedTo)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
         
            var response = await httpClient.GetAsync(new Uri(_archiveCoursesExceptBulkUploadReadytoGoLiveUri.AbsoluteUri + "?UKPRN=" + UKPRN + "&StatusToBeChangedTo=" + StatusToBeChangedTo));

            if (response.IsSuccessStatusCode)
            {
                return Result.Ok();
            }
            else
            {
                return Result.Fail("ChangeCourseRunStatusesForUKPRNSelection service unsuccessful http response");
            }
        }

        private static string ToJson(GetCourseByIdCriteria criteria)
        {
            GetCourseByIdJson json = new GetCourseByIdJson
            {
                id = criteria.Id.ToString()
            };
            
            return JsonConvert.SerializeObject(json);
        }

        private class GetCourseByIdJson
        {
            public string id { get; set; }
        }
    }
}
