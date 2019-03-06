
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Newtonsoft.Json;
using Dfc.CourseDirectory.Models.Models.Courses;
using System.Net;
using System.Text.RegularExpressions;
using Dfc.CourseDirectory.Common.Settings;
using System.Linq;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseService : ICourseService
    {
        private readonly ILogger<CourseService> _logger;
        private readonly HttpClient _httpClient;
        private readonly Uri _addCourseUri;
        private readonly Uri _getYourCoursesUri;
        private readonly Uri _updateCourseUri;
        private readonly Uri _getCourseByIdUri;

        private readonly int _courseForTextFieldMaxChars;
        private readonly int _entryRequirementsTextFieldMaxChars;
        private readonly int _whatWillLearnTextFieldMaxChars;
        private readonly int _howYouWillLearnTextFieldMaxChars;
        private readonly int _whatYouNeedTextFieldMaxChars;
        private readonly int _howAssessedTextFieldMaxChars;
        private readonly int _whereNextTextFieldMaxChars;

        public CourseService(
            ILogger<CourseService> logger,
            HttpClient httpClient,
            IOptions<CourseServiceSettings> settings,
            IOptions<CourseForComponentSettings> courseForComponentSettings,
            IOptions<EntryRequirementsComponentSettings> entryRequirementsComponentSettings,
            IOptions<WhatWillLearnComponentSettings> whatWillLearnComponentSettings,
            IOptions<HowYouWillLearnComponentSettings> howYouWillLearnComponentSettings,
            IOptions<WhatYouNeedComponentSettings> whatYouNeedComponentSettings,
            IOptions<HowAssessedComponentSettings> howAssessedComponentSettings,
            IOptions<WhereNextComponentSettings> whereNextComponentSettings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(httpClient, nameof(httpClient));
            Throw.IfNull(settings, nameof(settings));
            Throw.IfNull(courseForComponentSettings, nameof(courseForComponentSettings));
            Throw.IfNull(entryRequirementsComponentSettings, nameof(entryRequirementsComponentSettings));
            Throw.IfNull(whatWillLearnComponentSettings, nameof(whatWillLearnComponentSettings));
            Throw.IfNull(howYouWillLearnComponentSettings, nameof(howYouWillLearnComponentSettings));
            Throw.IfNull(whatYouNeedComponentSettings, nameof(whatYouNeedComponentSettings));
            Throw.IfNull(howAssessedComponentSettings, nameof(howAssessedComponentSettings));
            Throw.IfNull(whereNextComponentSettings, nameof(whereNextComponentSettings));


            _logger = logger;
            _httpClient = httpClient;

            _addCourseUri = settings.Value.ToAddCourseUri();
            _getYourCoursesUri = settings.Value.ToGetYourCoursesUri();
            _updateCourseUri = settings.Value.ToUpdateCourseUri();
            _getCourseByIdUri = settings.Value.ToGetCourseByIdUri();

            _courseForTextFieldMaxChars = courseForComponentSettings.Value.TextFieldMaxChars;
            _entryRequirementsTextFieldMaxChars = entryRequirementsComponentSettings.Value.TextFieldMaxChars;
            _whatWillLearnTextFieldMaxChars = whatWillLearnComponentSettings.Value.TextFieldMaxChars;
            _howYouWillLearnTextFieldMaxChars = howYouWillLearnComponentSettings.Value.TextFieldMaxChars;
            _whatYouNeedTextFieldMaxChars = whatYouNeedComponentSettings.Value.TextFieldMaxChars;
            _howAssessedTextFieldMaxChars = howAssessedComponentSettings.Value.TextFieldMaxChars;
            _whereNextTextFieldMaxChars = whereNextComponentSettings.Value.TextFieldMaxChars;
        }

        public SelectRegionModel GetRegions()
        {
            return new SelectRegionModel
            {
                LabelText = "Select course region",
                HintText = "For example, South West",
                AriaDescribedBy = "Select all that apply.",
                RegionItems = new RegionItemModel[]
                {
                    new RegionItemModel
                    {
                        Id = "E12000001",
                        Checked = false,
                        RegionName = "North East"
                    },
                    new RegionItemModel
                    {
                        Id = "E12000002",
                        Checked = false,
                        RegionName = "North West"
                    },
                    new RegionItemModel
                    {
                        Id = "E12000003",
                        Checked = false,
                        RegionName = "Yorkshire and The Humber"
                    },
                    new RegionItemModel
                    {
                        Id = "E12000004",
                        Checked = false,
                        RegionName = "East Midlands"
                    },
                    new RegionItemModel
                    {
                        Id = "E12000005",
                        Checked = false,
                        RegionName = "West Midlands"
                    },
                    new RegionItemModel
                    {
                        Id = "E12000006",
                        Checked = false,
                        RegionName = "East of England"
                    },
                    new RegionItemModel
                    {
                        Id = "E12000007",
                        Checked = false,
                        RegionName = "London"
                    },
                    new RegionItemModel
                    {
                        Id = "E12000008",
                        Checked = false,
                        RegionName = "South East"
                    },
                    new RegionItemModel
                    {
                        Id = "E12000009",
                        Checked = false,
                        RegionName = "South West"
                    }

                }
            };
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

                var response = await _httpClient.GetAsync(new Uri(_getCourseByIdUri.AbsoluteUri + "&id=" + criteria.Id));

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

        public async Task<IResult<ICourseSearchResult>> GetYourCoursesByUKPRNAsync(ICourseSearchCriteria criteria)
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

                var response = await _httpClient.GetAsync(new Uri(_getYourCoursesUri.AbsoluteUri + "&UKPRN=" + criteria.UKPRN));
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
                return Result.Fail<ICourseSearchResult>("Get your courses service unknown error.");

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

                var response = await _httpClient.GetAsync(new Uri(_getYourCoursesUri.AbsoluteUri + "&UKPRN=" + criteria.UKPRN));
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

        public IList<string> ValidateCourse(ICourse course)
        {
            var validationMessages = new List<string>();

            // CourseDescription
            if (string.IsNullOrEmpty(course.CourseDescription))
            {
                validationMessages.Add("Course For decription is required");
            }
            else
            {
                if (!HasOnlyFollowingValidCharacters(course.CourseDescription))
                    validationMessages.Add("Course For decription contains invalid character");
                if (course.CourseDescription.Length > _courseForTextFieldMaxChars)
                    validationMessages.Add($"Course For decription must be { _courseForTextFieldMaxChars } characters or less");
            }

            // EntryRequirements
            if (!string.IsNullOrEmpty(course.EntryRequirements))
            {
                if (!HasOnlyFollowingValidCharacters(course.EntryRequirements))
                    validationMessages.Add("Entry Requirements contains invalid character");
                if (course.EntryRequirements.Length > _entryRequirementsTextFieldMaxChars)
                    validationMessages.Add($"Entry Requirements must be { _entryRequirementsTextFieldMaxChars } characters or less");
            }

            // WhatYoullLearn 
            if (!string.IsNullOrEmpty(course.WhatYoullLearn))
            {
                if (!HasOnlyFollowingValidCharacters(course.WhatYoullLearn))
                    validationMessages.Add("What You'll Learn contains invalid character");
                if (course.WhatYoullLearn.Length > _whatWillLearnTextFieldMaxChars)
                    validationMessages.Add($"What You'll Learn must be { _whatWillLearnTextFieldMaxChars } characters or less");
            }

            // HowYoullLearn 
            if (!string.IsNullOrEmpty(course.HowYoullLearn))
            {
                if (!HasOnlyFollowingValidCharacters(course.HowYoullLearn))
                    validationMessages.Add("How You'll Learn contains invalid character");
                if (course.HowYoullLearn.Length > _howYouWillLearnTextFieldMaxChars)
                    validationMessages.Add($"How You'll Learn must be { _howYouWillLearnTextFieldMaxChars } characters or less");
            }

            // WhatYoullNeed 
            if (!string.IsNullOrEmpty(course.WhatYoullNeed))
            {
                if (!HasOnlyFollowingValidCharacters(course.WhatYoullNeed))
                    validationMessages.Add("What You'll Need contains invalid character");
                if (course.WhatYoullNeed.Length > _whatYouNeedTextFieldMaxChars)
                    validationMessages.Add($"What You'll Need must be { _whatYouNeedTextFieldMaxChars } characters or less");
            }

            // HowYoullBeAssessed 
            if (!string.IsNullOrEmpty(course.HowYoullBeAssessed))
            {
                if (!HasOnlyFollowingValidCharacters(course.HowYoullBeAssessed))
                    validationMessages.Add("How You'll Be Assessed contains invalid character");
                if (course.HowYoullBeAssessed.Length > _howAssessedTextFieldMaxChars)
                    validationMessages.Add($"How You'll Be Assessed must be { _howAssessedTextFieldMaxChars } characters or less");
            }

            // WhereNext 
            if (!string.IsNullOrEmpty(course.WhereNext))
            {
                if (!HasOnlyFollowingValidCharacters(course.WhereNext))
                    validationMessages.Add("Where Next contains invalid character");
                if (course.WhereNext.Length > _whereNextTextFieldMaxChars)
                    validationMessages.Add($"Where Next must be { _whereNextTextFieldMaxChars } characters or less");
            }

            return validationMessages;
        }

        public IList<string> ValidateCourseRun(ICourseRun courseRun, ValidationMode validationMode)
        {
            var validationMessages = new List<string>();

            // CourseName
            if (string.IsNullOrEmpty(courseRun.CourseName))
            {
                validationMessages.Add("Course Name is required"); // "Enter Course Name"
            }
            else
            {
                if (!HasOnlyFollowingValidCharacters(courseRun.CourseName))
                    validationMessages.Add("Course Name contains invalid character");
                if (courseRun.CourseName.Length > 255)
                    validationMessages.Add($"Course Name must be 255 characters or less");
            }

            // ProviderCourseID
            if (!string.IsNullOrEmpty(courseRun.ProviderCourseID))
            {
                if (!HasOnlyFollowingValidCharacters(courseRun.ProviderCourseID))
                    validationMessages.Add("ID contains invalid characters");
                if (courseRun.ProviderCourseID.Length > 255)
                    validationMessages.Add($"The maximum length of 'ID' is 255 characters");
            }

            // DeliveryMode
            switch (courseRun.DeliveryMode)
            {
                case DeliveryMode.ClassroomBased:

                    // VenueId
                    if (courseRun.VenueId == null || courseRun.VenueId == Guid.Empty)
                        validationMessages.Add($"Select a venue");

                    // StudyMode
                    if (courseRun.StudyMode.Equals(StudyMode.Undefined))
                        validationMessages.Add($"Select Study Mode");

                    // AttendancePattern
                    if (courseRun.AttendancePattern.Equals(AttendancePattern.Undefined))
                        validationMessages.Add($"Select Attendance Pattern");

                    break;
                case DeliveryMode.Online:
                    // No Specific Fields
                    break;
                case DeliveryMode.WorkBased:

                    // Regions
                    if (courseRun.Regions == null || courseRun.Regions.Count().Equals(0))
                        validationMessages.Add($"Select a region");
                    break;
                case DeliveryMode.Undefined: // Question ???
                default:
                    validationMessages.Add($"DeliveryMode is Undefined. We are not checking the specific fields now. On editing you can select the appropriate Delivery Mode and the rest of the fields will be validated accordingly.");
                    break;
            }

            // StartDate & FlexibleStartDate
            if (courseRun.StartDate != null)
            {
                switch (validationMode)
                {
                    case ValidationMode.AddCourse:
                        if (courseRun.StartDate < DateTime.Now || courseRun.StartDate > DateTime.Now.AddYears(2))
                            validationMessages.Add($"Start Date should not be in the past and not more than 2 years in the future");
                        break;
                    case ValidationMode.EditCourse:
                        //
                        break;
                    case ValidationMode.BulkUploadCourse:
                        if (courseRun.StartDate < DateTime.Now.AddMonths(-3) || courseRun.StartDate > DateTime.Now.AddYears(2))
                            validationMessages.Add($"Start Date should not be older than 3 months and not more than 2 years in the future");
                        break;
                    case ValidationMode.MigrateCourse:
                        if (courseRun.StartDate < DateTime.Now.AddMonths(-3) || courseRun.StartDate > DateTime.Now.AddYears(2))
                            validationMessages.Add($"Start Date should not be older than 3 months and not more than 2 years in the future");
                        break;
                    case ValidationMode.Undefined: // Question ???
                    default:
                        validationMessages.Add($"Validation Mode was not defined.");
                        break;
                }
            }

            if (courseRun.StartDate == null && courseRun.FlexibleStartDate == false)
                validationMessages.Add($"Either 'Defined Start Date' or 'Flexible Start Date' has to be provided");

            // CourseURL
            if (!string.IsNullOrEmpty(courseRun.CourseURL))
            {
                if (!IsValidUrl(courseRun.CourseURL))
                    validationMessages.Add("The format of URL is incorrect");
                if (courseRun.CourseURL.Length > 255)
                    validationMessages.Add($"The maximum length of URL is 255 characters");
            }

            // Cost & CostDescription
            if (string.IsNullOrEmpty(courseRun.CostDescription) && courseRun.Cost.Equals(null))
                validationMessages.Add($"Enter cost or cost description");

            if (!string.IsNullOrEmpty(courseRun.CostDescription))
            {
                if (!HasOnlyFollowingValidCharacters(courseRun.CostDescription))
                    validationMessages.Add("Cost Description contains invalid characters");
                if (courseRun.CostDescription.Length > 255)
                    validationMessages.Add($"Cost description must be 255 characters or less");
            }

            if (!courseRun.Cost.Equals(null))
            {
                if (!IsCorrectCostFormatting(courseRun.Cost.ToString()))
                    validationMessages.Add($"Enter the cost in pounds and pence");
            }

            // DurationUnit
            if (courseRun.DurationUnit.Equals(DurationUnit.Undefined))
                validationMessages.Add($"Select Duration Unit");

            // DurationValue
            if (courseRun.DurationValue.Equals(null))
            {
                validationMessages.Add($"Enter Duration");
            }
            else
            {
                if (!ValidDurationValue(courseRun.DurationValue?.ToString()))
                    validationMessages.Add("Duration must be numeric and maximum length is 3 digits");
            }

            return validationMessages;
        }

        public bool HasOnlyFollowingValidCharacters(string value)
        {
            string regex = @"^[a-zA-Z0-9 /\n/\r/\¬\!\£\$\%\^\&\*\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`" + "\"" + "\\\\]+$";
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
            return new Uri($"{extendee.ApiUrl + "AddCourse?code=" + extendee.ApiKey}");
        }

        internal static Uri ToGetYourCoursesUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "GetCoursesByLevelForUKPRN?code=" + extendee.ApiKey}");
        }

        internal static Uri ToUpdateCourseUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "UpdateCourse?code=" + extendee.ApiKey}");
        }

        internal static Uri ToGetCourseByIdUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "GetCourseById?code=" + extendee.ApiKey}");
        }
    }
}
