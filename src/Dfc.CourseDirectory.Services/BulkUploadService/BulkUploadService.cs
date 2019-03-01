using CsvHelper;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Services.Interfaces.BulkUploadService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System;
using Dfc.CourseDirectory.Models.Enums;
using System.ComponentModel;
using System.Globalization;
using Newtonsoft.Json;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Models.Models.Venues;

namespace Dfc.CourseDirectory.Services.BulkUploadService
{
    public class BulkUploadService : IBulkUploadService
    {
        private readonly ILogger<BulkUploadService> _logger;
        private readonly ILarsSearchSettings _larsSearchSettings;
        private readonly ILarsSearchService _larsSearchService;
        private readonly IVenueServiceSettings _venueServiceSettings;
        private readonly IVenueService _venueService;
        private readonly ICourseServiceSettings _courseServiceSettings;
        private readonly ICourseService _courseService;

        public BulkUploadService(
            ILogger<BulkUploadService> logger,
            IOptions<LarsSearchSettings> larsSearchSettings,
            ILarsSearchService larsSearchService,
            IOptions<VenueServiceSettings> venueServiceSettings,
            IVenueService venueService,
            IOptions<CourseServiceSettings> courseServiceSettings,
            ICourseService courseService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(larsSearchSettings, nameof(larsSearchSettings));
            Throw.IfNull(larsSearchService, nameof(larsSearchService));
            Throw.IfNull(venueServiceSettings, nameof(venueServiceSettings));
            Throw.IfNull(venueService, nameof(venueService));
            Throw.IfNull(courseServiceSettings, nameof(courseServiceSettings));
            Throw.IfNull(courseService, nameof(courseService));

            _logger = logger;
            _larsSearchSettings = larsSearchSettings.Value;
            _larsSearchService = larsSearchService;
            _venueServiceSettings = venueServiceSettings.Value;
            _venueService = venueService;
            _courseServiceSettings = courseServiceSettings.Value;
            _courseService = courseService;
        }

        public List<string> ProcessBulkUpload(string bulkUploadFilePath, int providerUKPRN, string userId)
        {
            var errors = new List<string>();
            var bulkUploadcourses = new List<BulkUploadCourse>();
            int bulkUploadLineNumber = 2;
            string previousLearnAimRef = string.Empty;
            List<int> lineNumbersInCourse = new List<int>();

            try
            {
                using (var reader = new StreamReader(bulkUploadFilePath))
                {
                    using (var csv = new CsvReader(reader))
                    {
                        csv.Read();
                        csv.ReadHeader();
                        while (csv.Read())
                        {
                            bool isCourseHeader = false;
                            string currentLearnAimRef = csv.GetField("LARS_QAN").Trim();

                            if (bulkUploadLineNumber.Equals(2) || currentLearnAimRef != previousLearnAimRef)
                            {
                                isCourseHeader = true;
                            }

                            if (string.IsNullOrEmpty(currentLearnAimRef))
                            {
                                errors.Add($"Line { bulkUploadLineNumber }, LARS_QAN = { currentLearnAimRef } => LARS is missing.");
                            }
                            else
                            {
                                var record = new BulkUploadCourse
                                {
                                    IsCourseHeader = isCourseHeader,
                                    BulkUploadLineNumber = bulkUploadLineNumber,
                                    LearnAimRef = currentLearnAimRef,
                                    ProviderUKPRN = providerUKPRN,
                                    VenueName = csv.GetField("VENUE").Trim(),
                                    CourseName = csv.GetField("COURSE_NAME").Trim(),
                                    ProviderCourseID = csv.GetField("ID").Trim(),
                                    DeliveryMode = csv.GetField("DELIVERY_MODE").Trim(),
                                    FlexibleStartDate = csv.GetField("FLEXIBLE_START_DATE").Trim(),
                                    StartDate = csv.GetField("START_DATE").Trim(),
                                    CourseURL = csv.GetField("URL").Trim(),
                                    Cost = csv.GetField("COST").Trim(),
                                    CostDescription = csv.GetField("COST_DESCRIPTION").Trim(),
                                    DurationUnit = csv.GetField("DURATION_UNIT").Trim(),
                                    DurationValue = csv.GetField("DURATION").Trim(),
                                    StudyMode = csv.GetField("STUDY_MODE").Trim(),
                                    AttendancePattern = csv.GetField("ATTENDANCE_PATTERN").Trim()
                                };

                                if (isCourseHeader)
                                {
                                    record.CourseDescription = csv.GetField("WHO_IS_THIS_COURSE_FOR").Trim();
                                    record.EntryRequirements = csv.GetField("ENTRY_REQUIREMENTS").Trim();
                                    record.WhatYoullLearn = csv.GetField("WHAT_YOU_WILL_LEARN").Trim();
                                    record.HowYoullLearn = csv.GetField("HOW_YOU_WILL_LEARN").Trim();
                                    record.WhatYoullNeed = csv.GetField("WHAT_YOU_WILL_NEED_TO_BRING").Trim();
                                    record.HowYoullBeAssessed = csv.GetField("HOW_YOU_WILL_BE_ASSESSED").Trim();
                                    record.WhereNext = csv.GetField("WHERE_NEXT").Trim();
                                    record.AdultEducationBudget = csv.GetField("ADULT_EDUCATION_BUDGET").Trim();
                                    record.AdvancedLearnerLoan = csv.GetField("ADVANCED_LEARNER_OPTION").Trim();
                                }

                                bulkUploadcourses.Add(record);
                            }

                            previousLearnAimRef = currentLearnAimRef;
                            bulkUploadLineNumber++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Problem  => Error - { ex.Message }");
            }

            if (errors != null && errors.Count > 0)
            {
                return errors;
            }
            else
            {
                if (bulkUploadcourses == null || bulkUploadcourses.Count.Equals(0))
                {
                    errors.Add($"You have uploaded an empty file.");
                    return errors;
                }

                // Populate LARS data
                bulkUploadcourses = PolulateLARSData(bulkUploadcourses, out errors);
                if (errors != null && errors.Count > 0)
                {
                    // If we have invalid LARS we stop processing
                    return errors;
                }
                else
                {
                    // Mapping BulkUploadCourse to Course
                    var courses = MappingBulkUploadCourseToCourse(bulkUploadcourses, userId, out errors);


                    return errors;
                }
            }
        }

        public List<BulkUploadCourse> PolulateLARSData(List<BulkUploadCourse> bulkUploadcourses, out List<string> errors)
        {
            errors = new List<string>();

            foreach (var bulkUploadcourse in bulkUploadcourses.Where(c => c.IsCourseHeader == true).ToList())
            {
                LarsSearchCriteria criteria = new LarsSearchCriteria(bulkUploadcourse.LearnAimRef, 10, 0, string.Empty, null);
                var larsResult = Task.Run(async () => await _larsSearchService.SearchAsync(criteria)).Result;

                if (larsResult.IsSuccess && larsResult.HasValue)
                {
                    var qualifications = new List<LarsDataResultItem>();
                    foreach (var item in larsResult.Value.Value)
                    {
                        var larsDataResultItem = new LarsDataResultItem
                        {
                            LearnAimRef = item.LearnAimRef,
                            LearnAimRefTitle = item.LearnAimRefTitle,
                            NotionalNVQLevelv2 = item.NotionalNVQLevelv2,
                            AwardOrgCode = item.AwardOrgCode,
                            LearnAimRefTypeDesc = item.LearnAimRefTypeDesc,
                            CertificationEndDate = item.CertificationEndDate
                        };
                        qualifications.Add(larsDataResultItem);
                    }

                    if (qualifications.Count.Equals(0))
                    {
                        List<int> invalidLARSLineNumbers = bulkUploadcourses.Where(c => c.LearnAimRef == bulkUploadcourse.LearnAimRef).Select(l => l.BulkUploadLineNumber).ToList();
                        errors.Add($"{ InvalidLARSLineNumbersToString(invalidLARSLineNumbers) }, LARS_QAN = { bulkUploadcourse.LearnAimRef } invalid LARS");
                    }
                    else if (qualifications.Count.Equals(1))
                    {
                        if (qualifications[0].CertificationEndDate != null && qualifications[0].CertificationEndDate < DateTime.Now)
                        {
                            List<int> invalidLARSLineNumbers = bulkUploadcourses.Where(c => c.LearnAimRef == bulkUploadcourse.LearnAimRef).Select(l => l.BulkUploadLineNumber).ToList();
                            errors.Add($"{ InvalidLARSLineNumbersToString(invalidLARSLineNumbers) }, LARS_QAN = { bulkUploadcourse.LearnAimRef } expired LARS");
                        }
                        else
                        {
                            bulkUploadcourse.QualificationCourseTitle = qualifications[0].LearnAimRefTitle;
                            bulkUploadcourse.NotionalNVQLevelv2 = qualifications[0].NotionalNVQLevelv2;
                            bulkUploadcourse.AwardOrgCode = qualifications[0].AwardOrgCode;
                            bulkUploadcourse.QualificationType = qualifications[0].LearnAimRefTypeDesc;
                        }
                    }
                    else
                    {
                        string logMoreQualifications = string.Empty;
                        foreach (var qualification in qualifications)
                        {
                            logMoreQualifications += "( '" + qualification.LearnAimRefTitle + "' with Level " + qualification.NotionalNVQLevelv2 + " and AwardOrgCode " + qualification.AwardOrgCode + " ) ";
                        }
                        errors.Add($"We retrieve multiple qualifications ( { qualifications.Count.ToString() } ) for the LARS { bulkUploadcourse.LearnAimRef }, which are { logMoreQualifications } ");
                    }
                }
                else
                {
                    errors.Add($"We couldn't retreive LARS data for LARS { bulkUploadcourse.LearnAimRef }, because of technical reason, Error: " + larsResult?.Error);
                }
            }

            return bulkUploadcourses;
        }

        public List<Course> MappingBulkUploadCourseToCourse(List<BulkUploadCourse> bulkUploadcourses, string userId, out List<string> errors)
        {
            errors = new List<string>();
            var validationMessages = new List<string>();

            var courses = new List<Course>();       
            var listsCourseRuns = new List<BulkUploadCourseRun>();

            foreach (var bulkUploadcourse in bulkUploadcourses)
            {
                if (bulkUploadcourse.IsCourseHeader)
                {
                    var course = new Course();
                    course.id = Guid.NewGuid();
                    course.QualificationCourseTitle = bulkUploadcourse.QualificationCourseTitle;
                    course.LearnAimRef = bulkUploadcourse.LearnAimRef;
                    course.NotionalNVQLevelv2 = bulkUploadcourse.NotionalNVQLevelv2;
                    course.AwardOrgCode = bulkUploadcourse.AwardOrgCode;
                    course.QualificationType = bulkUploadcourse.QualificationType;
                    course.ProviderUKPRN = bulkUploadcourse.ProviderUKPRN;
                    course.CourseDescription = bulkUploadcourse.CourseDescription;
                    course.EntryRequirements = bulkUploadcourse.EntryRequirements;
                    course.WhatYoullLearn = bulkUploadcourse.WhatYoullLearn;
                    course.HowYoullLearn = bulkUploadcourse.HowYoullLearn;
                    course.WhatYoullNeed = bulkUploadcourse.WhatYoullNeed;
                    course.HowYoullBeAssessed = bulkUploadcourse.HowYoullBeAssessed;
                    course.WhereNext = bulkUploadcourse.WhereNext;
                    course.AdvancedLearnerLoan = bulkUploadcourse.AdvancedLearnerLoan.Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                    course.AdultEducationBudget = bulkUploadcourse.AdultEducationBudget.Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;

                    course.CreatedBy = userId;
                    course.CreatedDate = DateTime.Now;

                    courses.Add(course);
                }

                var courseRun = new CourseRun();
                courseRun.id = Guid.NewGuid();
                courseRun.RecordStatus = RecordStatus.BulkUploadReadyToGoLive;

                courseRun.DeliveryMode = GetValueFromDescription<DeliveryMode>(bulkUploadcourse.DeliveryMode);
                if (courseRun.DeliveryMode.Equals(DeliveryMode.Undefined))
                {
                    courseRun.RecordStatus = RecordStatus.BulkUloadPending;
                    validationMessages.Add($"DeliveryMode is Undefined, because you have entered ( { bulkUploadcourse.DeliveryMode } ), Line { bulkUploadcourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadcourse.LearnAimRef }, ID = { bulkUploadcourse.ProviderCourseID }");
                }

                // Call VenueService and for VenueName get VenueId (GUID) (Applicable only for type ClassroomBased)
                if (courseRun.DeliveryMode.Equals(DeliveryMode.ClassroomBased))
                {
                    if (string.IsNullOrEmpty(bulkUploadcourse.VenueName))
                    {
                        courseRun.RecordStatus = RecordStatus.BulkUloadPending;
                        validationMessages.Add($"NO Venue Name for Line { bulkUploadcourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadcourse.LearnAimRef }, ID = { bulkUploadcourse.ProviderCourseID }");

                        
                    }
                    else
                    {
                        GetVenuesByPRNAndNameCriteria venueCriteria = new GetVenuesByPRNAndNameCriteria(bulkUploadcourse.ProviderUKPRN.ToString(), bulkUploadcourse.VenueName);
                        var venueResult = Task.Run(async () => await _venueService.GetVenuesByPRNAndNameAsync(venueCriteria)).Result;

                        if (venueResult.IsSuccess && venueResult.HasValue)
                        {
                            var venues = (IEnumerable<Venue>)venueResult.Value.Value;
                            if (venues.Count().Equals(1))
                            {
                                courseRun.VenueId = new Guid(venues.FirstOrDefault().ID);
                            }
                            else
                            {
                                courseRun.RecordStatus = RecordStatus.BulkUloadPending;
                                validationMessages.Add($"We have obtained muliple Venues for { bulkUploadcourse.VenueName } - Line { bulkUploadcourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadcourse.LearnAimRef }, ID = { bulkUploadcourse.ProviderCourseID }");
                            }
                        }
                        else
                        {
                            courseRun.RecordStatus = RecordStatus.BulkUloadPending;
                            validationMessages.Add($"We could NOT obtain a Venue for { bulkUploadcourse.VenueName } - Line { bulkUploadcourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadcourse.LearnAimRef }, ID = { bulkUploadcourse.ProviderCourseID }");
                        }
                    }
                }

                courseRun.CourseName = bulkUploadcourse.CourseName;
                courseRun.ProviderCourseID = bulkUploadcourse.ProviderCourseID;
                
                courseRun.FlexibleStartDate = bulkUploadcourse.FlexibleStartDate.Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;

                DateTime specifiedStartDate;
                if (DateTime.TryParseExact(bulkUploadcourse.StartDate, "dd/MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out specifiedStartDate))
                {
                    courseRun.StartDate = specifiedStartDate;
                }
                else
                {
                    courseRun.StartDate = null;
                    validationMessages.Add($"StartDate is NULL, because you have entered ( { bulkUploadcourse.StartDate } ), Line { bulkUploadcourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadcourse.LearnAimRef }, ID = { bulkUploadcourse.ProviderCourseID }");
                }

                courseRun.CourseURL = bulkUploadcourse.CourseURL;

                decimal specifiedCost;
                if (decimal.TryParse(bulkUploadcourse.Cost, out specifiedCost))
                {
                    courseRun.Cost = specifiedCost;
                }
                else
                {
                    courseRun.Cost = null;
                    validationMessages.Add($"Cost is NULL, because you have entered ( { bulkUploadcourse.Cost } ), Line { bulkUploadcourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadcourse.LearnAimRef }, ID = { bulkUploadcourse.ProviderCourseID }");
                }

                courseRun.CostDescription = bulkUploadcourse.CostDescription;
                courseRun.DurationUnit = GetValueFromDescription<DurationUnit>(bulkUploadcourse.DurationUnit);
                if (courseRun.DurationUnit.Equals(DurationUnit.Undefined))
                {
                    courseRun.RecordStatus = RecordStatus.BulkUloadPending;
                    validationMessages.Add($"DurationUnit is Undefined, because you have entered ( { bulkUploadcourse.DurationUnit } ), Line { bulkUploadcourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadcourse.LearnAimRef }, ID = { bulkUploadcourse.ProviderCourseID }");
                }
                
                int specifiedDurationValue;
                if (int.TryParse(bulkUploadcourse.DurationValue, out specifiedDurationValue))
                {
                    courseRun.DurationValue = specifiedDurationValue;
                }
                else
                {
                    courseRun.DurationValue = null;
                    validationMessages.Add($"DurationValue is NULL, because you have entered ( { bulkUploadcourse.DurationValue } ), Line { bulkUploadcourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadcourse.LearnAimRef }, ID = { bulkUploadcourse.ProviderCourseID }");
                }

                courseRun.StudyMode = GetValueFromDescription<StudyMode>(bulkUploadcourse.StudyMode);
                if (courseRun.StudyMode.Equals(StudyMode.Undefined))
                {
                    courseRun.RecordStatus = RecordStatus.BulkUloadPending;
                    validationMessages.Add($"StudyMode is Undefined, because you have entered ( { bulkUploadcourse.StudyMode } ), Line { bulkUploadcourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadcourse.LearnAimRef }, ID = { bulkUploadcourse.ProviderCourseID }");
                }
                courseRun.AttendancePattern = GetValueFromDescription<AttendancePattern>(bulkUploadcourse.AttendancePattern);
                if (courseRun.AttendancePattern.Equals(AttendancePattern.Undefined))
                {
                    courseRun.RecordStatus = RecordStatus.BulkUloadPending;
                    validationMessages.Add($"AttendancePattern is Undefined, because you have entered ( { bulkUploadcourse.AttendancePattern } ), Line { bulkUploadcourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadcourse.LearnAimRef }, ID = { bulkUploadcourse.ProviderCourseID }");
                }

                courseRun.CreatedBy = userId;
                courseRun.CreatedDate = DateTime.Now;

                listsCourseRuns.Add(new BulkUploadCourseRun { LearnAimRef = bulkUploadcourse.LearnAimRef, CourseRun = courseRun } );
            }

            foreach(var course in courses)
            {
                course.CourseRuns = listsCourseRuns.Where(cr => cr.LearnAimRef == course.LearnAimRef).Select(cr => cr.CourseRun).ToList();
            }

            //// Uncomment only for DEV and TESTING
            //foreach (var course in courses)
            //{
            //    string jsonBulkUploadCoursesFilesPath = "D:\\FindACourse-BulkUploadJSONfiles";
            //    var courseJson = JsonConvert.SerializeObject(course);
            //    string jsonFileName = string.Format("{0}-{1}-{2}-{3}-{4}.json", DateTime.Now.ToString("yyMMdd-HHmmss"), course.ProviderUKPRN, course.LearnAimRef, course.CourseId, course.CourseRuns.Count().ToString());
            //    File.WriteAllText(string.Format(@"{0}\{1}", jsonBulkUploadCoursesFilesPath, jsonFileName), courseJson);
            //}

            // Push the courses to the CourseService
            foreach (var course in courses)
            {
                var courseResult = Task.Run(async () => await _courseService.AddCourseAsync(course)).Result;

                if (courseResult.IsSuccess && courseResult.HasValue)
                {
                    //CountProviderCourseMigrationSuccess++;
                    //courseReport += $"The course is migarted  " + Environment.NewLine;
                    //migrationSuccess = MigrationSuccess.Success;
                }
                else
                {
                    errors.Add($"The course is NOT migrated, LARS_QAN = { course.LearnAimRef }. Error -  { courseResult.Error }");
                }
            }

            return courses;
        }

        public string InvalidLARSLineNumbersToString(List<int> invalidLARSLineNumbers)
        {
            string invalidLARSLineNumbersToString = string.Empty;

            if (invalidLARSLineNumbers.Count.Equals(1))
                invalidLARSLineNumbersToString = $"Line { invalidLARSLineNumbers[0] }";
            if (invalidLARSLineNumbers.Count > 1)
            {
                int lastNumber = invalidLARSLineNumbers[invalidLARSLineNumbers.Count - 1];
                invalidLARSLineNumbers.RemoveAt(invalidLARSLineNumbers.Count - 1);
                invalidLARSLineNumbersToString = "Lines ";
                invalidLARSLineNumbersToString += string.Join(", ", invalidLARSLineNumbers);
                invalidLARSLineNumbersToString += " and " + lastNumber;
            }

            return invalidLARSLineNumbersToString;
        }

        public static T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum)
                return default(T);

            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }

            return default(T);
        }
    }
}
