using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces.BulkUploadService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Services.Models.Regions;
using Dfc.CourseDirectory.Services.Models.Venues;
using Dfc.CourseDirectory.Services.VenueService;
using Microsoft.Extensions.Options;
using static Dfc.CourseDirectory.Services.Enums.AlternativeName;

namespace Dfc.CourseDirectory.Services.BulkUploadService
{
    public class BulkUploadService : IBulkUploadService
    {
        private readonly IVenueService _venueService;
        private readonly ICourseServiceSettings _courseServiceSettings;
        private readonly ICourseService _courseService;
        private readonly ISearchClient<Lars> _larsSearchClient;

        private List<Venue> cachedVenues;

        public BulkUploadService(
            IVenueService venueService,
            IOptions<CourseServiceSettings> courseServiceSettings,
            ICourseService courseService,
            ISearchClient<Lars> larsSearchClient)
        {
            _venueService = venueService ?? throw new ArgumentNullException(nameof(venueService));
            _courseServiceSettings = courseServiceSettings?.Value ?? throw new ArgumentNullException(nameof(courseServiceSettings));
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _larsSearchClient = larsSearchClient ?? throw new ArgumentNullException(nameof(larsSearchClient));
        }

        public int BulkUploadSecondsPerRecord
        {
            get
            {
                int spr = Math.Max(1, _courseServiceSettings.BulkUploadSecondsPerRecord);
                return spr;
            }
        }

        public int CountCsvLines(Stream stream)
        {
            int count = 0;

            stream.Position = 0;
            StreamReader sr = new StreamReader(stream);  // don't dispose the stream we'll need it later.
            while (sr.ReadLine() != null)
            {
                ++count;
            }

            return count;
        }

        public List<string> ProcessBulkUpload(Stream stream, int providerUKPRN, string userId, bool uploadCourses)
        {
            var errors = new List<string>();
            var bulkUploadcourses = new List<BulkUploadCourse>();
            int bulkUploadLineNumber = 2;
            int tempCourseId = 0;
            string previousLearnAimRef = string.Empty;

            try {
                cachedVenues = Task.Run(async () => await _venueService.SearchAsync(new VenueSearchCriteria(providerUKPRN.ToString(), string.Empty)))
                                                                       .Result
                                                                       .Value
                                                                       .Value
                                                                       .ToList();
                string missingFieldsError = string.Empty;
                int missingFieldsErrorCount = 0;
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csv.Read();
                        csv.ReadHeader();
                        while (csv.Read())
                        {
                            // To enable multiple missing fields error display
                            if (bulkUploadLineNumber.Equals(2))
                            {
                                string larsQan = string.Empty;
                                if (!csv.TryGetField("LARS_QAN", out larsQan))
                                {
                                    missingFieldsError += " 'LARS_QAN',"; missingFieldsErrorCount++;
                                }
                                string venue = string.Empty;
                                if (!csv.TryGetField("VENUE", out venue))
                                {
                                    missingFieldsError += " 'VENUE',"; missingFieldsErrorCount++;
                                }
                                string COURSE_NAME = string.Empty;
                                if (!csv.TryGetField("COURSE_NAME", out COURSE_NAME))
                                {
                                    missingFieldsError += " 'COURSE_NAME',"; missingFieldsErrorCount++;
                                }
                                string ID = string.Empty;
                                if (!csv.TryGetField("ID", out venue))
                                {
                                    missingFieldsError += " 'ID',"; missingFieldsErrorCount++;
                                }
                                string DELIVERY_MODE = string.Empty;
                                if (!csv.TryGetField("DELIVERY_MODE", out larsQan))
                                {
                                    missingFieldsError += " 'DELIVERY_MODE',"; missingFieldsErrorCount++;
                                }
                                string FLEXIBLE_START_DATE = string.Empty;
                                if (!csv.TryGetField("FLEXIBLE_START_DATE", out venue))
                                {
                                    missingFieldsError += " 'FLEXIBLE_START_DATE',"; missingFieldsErrorCount++;
                                }
                                string START_DATE = string.Empty;
                                if (!csv.TryGetField("START_DATE", out venue))
                                {
                                    missingFieldsError += " 'START_DATE',"; missingFieldsErrorCount++;
                                }
                                string URL = string.Empty;
                                if (!csv.TryGetField("URL", out larsQan))
                                {
                                    missingFieldsError += " 'URL',"; missingFieldsErrorCount++;
                                }
                                string COST = string.Empty;
                                if (!csv.TryGetField("COST", out venue))
                                {
                                    missingFieldsError += " 'COST',"; missingFieldsErrorCount++;
                                }
                                string COST_DESCRIPTION = string.Empty;
                                if (!csv.TryGetField("COST_DESCRIPTION", out larsQan))
                                {
                                    missingFieldsError += " 'COST_DESCRIPTION',"; missingFieldsErrorCount++;
                                }
                                string DURATION_UNIT = string.Empty;
                                if (!csv.TryGetField("DURATION_UNIT", out venue))
                                {
                                    missingFieldsError += " 'DURATION_UNIT',"; missingFieldsErrorCount++;
                                }
                                string DURATION = string.Empty;
                                if (!csv.TryGetField("DURATION", out larsQan))
                                {
                                    missingFieldsError += " 'DURATION',"; missingFieldsErrorCount++;
                                }
                                string STUDY_MODE = string.Empty;
                                if (!csv.TryGetField("STUDY_MODE", out STUDY_MODE))
                                {
                                    missingFieldsError += " 'STUDY_MODE',"; missingFieldsErrorCount++;
                                }
                                string ATTENDANCE_PATTERN = string.Empty;
                                if (!csv.TryGetField("ATTENDANCE_PATTERN", out venue))
                                {
                                    missingFieldsError += " 'ATTENDANCE_PATTERN',"; missingFieldsErrorCount++;
                                }
                                string WHO_IS_THIS_COURSE_FOR = string.Empty;
                                if (!csv.TryGetField("WHO_IS_THIS_COURSE_FOR", out larsQan))
                                {
                                    missingFieldsError += " 'WHO_IS_THIS_COURSE_FOR',"; missingFieldsErrorCount++;
                                }
                                string ENTRY_REQUIREMENTS = string.Empty;
                                if (!csv.TryGetField("ENTRY_REQUIREMENTS", out venue))
                                {
                                    missingFieldsError += " 'ENTRY_REQUIREMENTS',"; missingFieldsErrorCount++;
                                }
                                string WHAT_YOU_WILL_LEARN = string.Empty;
                                if (!csv.TryGetField("WHAT_YOU_WILL_LEARN", out COURSE_NAME))
                                {
                                    missingFieldsError += " 'WHAT_YOU_WILL_LEARN',"; missingFieldsErrorCount++;
                                }
                                string HOW_YOU_WILL_LEARN = string.Empty;
                                if (!csv.TryGetField("HOW_YOU_WILL_LEARN", out venue))
                                {
                                    missingFieldsError += " 'HOW_YOU_WILL_LEARN',"; missingFieldsErrorCount++;
                                }
                                string WHAT_YOU_WILL_NEED_TO_BRING = string.Empty;
                                if (!csv.TryGetField("WHAT_YOU_WILL_NEED_TO_BRING", out larsQan))
                                {
                                    missingFieldsError += " 'WHAT_YOU_WILL_NEED_TO_BRING',"; missingFieldsErrorCount++;
                                }
                                string HOW_YOU_WILL_BE_ASSESSED = string.Empty;
                                if (!csv.TryGetField("HOW_YOU_WILL_BE_ASSESSED", out venue))
                                {
                                    missingFieldsError += " 'HOW_YOU_WILL_BE_ASSESSED',"; missingFieldsErrorCount++;
                                }
                                string WHERE_NEXT = string.Empty;
                                if (!csv.TryGetField("WHERE_NEXT", out larsQan))
                                {
                                    missingFieldsError += " 'WHERE_NEXT',"; missingFieldsErrorCount++;
                                }
                                string ADULT_EDUCATION_BUDGET = string.Empty;
                                if (!csv.TryGetField("ADULT_EDUCATION_BUDGET", out venue))
                                {
                                    missingFieldsError += " 'ADULT_EDUCATION_BUDGET',"; missingFieldsErrorCount++;
                                }
                                string ADVANCED_LEARNER_OPTION = string.Empty;
                                if (!csv.TryGetField("ADVANCED_LEARNER_OPTION", out larsQan))
                                {
                                    missingFieldsError += " 'ADVANCED_LEARNER_OPTION',"; missingFieldsErrorCount++;
                                }
                                if (!csv.TryGetField("NATIONAL_DELIVERY", out larsQan))
                                {
                                    missingFieldsError += " 'NATIONAL_DELIVERY',"; missingFieldsErrorCount++;
                                }
                                if (!csv.TryGetField("REGION", out larsQan))
                                {
                                    missingFieldsError += " 'REGION',"; missingFieldsErrorCount++;
                                }
                                if (!csv.TryGetField("SUB_REGION", out larsQan))
                                {
                                    missingFieldsError += " 'SUB_REGION',"; missingFieldsErrorCount++;
                                }
                            }

                            if (string.IsNullOrEmpty(missingFieldsError))
                            {
                                bool isCourseHeader = false;
                                string currentLearnAimRef = csv.GetField("LARS_QAN").Trim();
                                string courseFor = csv.GetField("WHO_IS_THIS_COURSE_FOR").Trim();

                                if (bulkUploadLineNumber.Equals(2) || currentLearnAimRef != previousLearnAimRef || !string.IsNullOrEmpty(courseFor))
                                {
                                    isCourseHeader = true;
                                    tempCourseId++;
                                }

                                if (string.IsNullOrEmpty(currentLearnAimRef))
                                    errors.Add($"Line { bulkUploadLineNumber }, LARS_QAN = { currentLearnAimRef } => LARS is missing.");

                                else {
                                    var record = new BulkUploadCourse
                                    {
                                        IsCourseHeader = isCourseHeader,
                                        TempCourseId = tempCourseId,
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
                                        AttendancePattern = csv.GetField("ATTENDANCE_PATTERN").Trim(),
                                        National = csv.GetField("NATIONAL_DELIVERY").Trim(),
                                        Regions = csv.GetField("REGION").Trim(),
                                        SubRegions = csv.GetField("SUB_REGION").Trim()
                                    };

                                    if (isCourseHeader)
                                    {
                                        record.CourseDescription = courseFor; //csv.GetField("WHO_IS_THIS_COURSE_FOR").Trim();
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
                            }
                            bulkUploadLineNumber++;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(missingFieldsError))
                {
                    missingFieldsError = missingFieldsError.TrimEnd(',');
                    if (missingFieldsErrorCount.Equals(1))
                        errors.Add($"Field with name { missingFieldsError } does not exist");
                    else
                        errors.Add($"Fields with names { missingFieldsError } do not exist");
                    return errors;
                }

            } catch (Exception ex) {
                string errorMessage = ex.Message;
                if (errorMessage.Contains("You can ignore missing fields by setting MissingFieldFound to null.", StringComparison.InvariantCultureIgnoreCase))
                    errorMessage = errorMessage.Replace("You can ignore missing fields by setting MissingFieldFound to null.", string.Empty);
                errors.Add($"We experienced an error. Text: { errorMessage }");
            }

            if (errors != null && errors.Count > 0)
                return errors;

            else {
                if (bulkUploadcourses == null || bulkUploadcourses.Count.Equals(0)) {
                    errors.Add($"The selected file is empty");
                    return errors;
                }

                if(uploadCourses)
                {
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
                else
                {
                    return errors;
                }
            }
        }

        public List<BulkUploadCourse> PolulateLARSData(List<BulkUploadCourse> bulkUploadcourses, out List<string> errors)
        {
            errors = new List<string>();
            var totalErrorList = new List<int>();
            
            var cachedQuals = new List<Lars>();

            foreach (var bulkUploadcourse in bulkUploadcourses.Where(c => c.IsCourseHeader == true))
            {
                var cachedResult = cachedQuals.FirstOrDefault(o => o.LearnAimRef == bulkUploadcourse.LearnAimRef);
                
                List<Lars> results = null;

                if (cachedResult == null)
                {
                    results = _larsSearchClient.Search(new LarsLearnAimRefSearchQuery
                    {
                        LearnAimRef = bulkUploadcourse.LearnAimRef
                    }).GetAwaiter().GetResult().Results.ToList();

                    var qual = results.FirstOrDefault();
                    
                    if (qual != null)
                    {
                        cachedQuals.Add(qual);
                    }
                }
                else
                {
                    results = new List<Lars> { cachedQuals.FirstOrDefault(o => o.LearnAimRef == bulkUploadcourse.LearnAimRef) };
                }

                if (results.Count > 0)
                {
                    if (results.Count.Equals(0))
                    {
                        List<int> invalidLARSLineNumbers = bulkUploadcourses.Where(c => c.LearnAimRef == bulkUploadcourse.LearnAimRef).Select(l => l.BulkUploadLineNumber).ToList();

                        invalidLARSLineNumbers = CheckForErrorDuplicates(ref totalErrorList, invalidLARSLineNumbers);

                        if (invalidLARSLineNumbers.Count > 0)
                        {
                            errors.Add($"{ InvalidLARSLineNumbersToString(invalidLARSLineNumbers) }, " + $"LARS_QAN = { bulkUploadcourse.LearnAimRef } invalid LARS");
                        }

                    }
                    else if (results.Count.Equals(1))
                    {
                        if (results[0].CertificationEndDate != null && results[0].CertificationEndDate < DateTime.Now)
                        {
                            List<int> invalidLARSLineNumbers = bulkUploadcourses.Where(c => c.LearnAimRef == bulkUploadcourse.LearnAimRef).Select(l => l.BulkUploadLineNumber).ToList();

                            invalidLARSLineNumbers = CheckForErrorDuplicates(ref totalErrorList, invalidLARSLineNumbers);

                            if (invalidLARSLineNumbers.Count > 0)
                            {
                                errors.Add($"{ InvalidLARSLineNumbersToString(invalidLARSLineNumbers) }, LARS_QAN = { bulkUploadcourse.LearnAimRef } expired LARS");
                            }

                        }
                        else
                        {
                            bulkUploadcourse.QualificationCourseTitle = results[0].LearnAimRefTitle;
                            bulkUploadcourse.NotionalNVQLevelv2 = results[0].NotionalNVQLevelv2;
                            bulkUploadcourse.AwardOrgCode = results[0].AwardOrgCode;
                            bulkUploadcourse.QualificationType = results[0].LearnAimRefTypeDesc;
                        }
                    }
                    else
                    {
                        string logMoreQualifications = string.Empty;
                        foreach (var qualification in results)
                        {
                            logMoreQualifications += "( '" + qualification.LearnAimRefTitle + "' with Level " + qualification.NotionalNVQLevelv2 + " and AwardOrgCode " + qualification.AwardOrgCode + " ) ";
                        }
                        errors.Add($"We retrieve multiple qualifications ( { results.Count } ) for the LARS { bulkUploadcourse.LearnAimRef }, which are { logMoreQualifications } ");
                    }
                }
                else
                {
                    errors.Add($"Line {bulkUploadcourse.BulkUploadLineNumber}, LARS_QAN = { bulkUploadcourse.LearnAimRef }, invalid LARS");
                }

            }

            return bulkUploadcourses;
        }

        public List<int> CheckForErrorDuplicates(ref List<int> totalList, List<int> errorList)
        {

            for (int i = errorList.Count - 1; i >= 0; i--)
            {
                bool exists = totalList.Any(x => x.Equals(errorList[i]));
                if (exists)
                {
                    errorList.Remove(errorList[i]);
                }
                else
                {
                    totalList.Add(errorList[i]);
                }
            }
            return errorList;
        }

        public List<Course> MappingBulkUploadCourseToCourse(List<BulkUploadCourse> bulkUploadCourses, string userId, out List<string> errors)
        {
            errors = new List<string>();
            var validationMessages = new List<string>();

            var courses = new List<Course>();
            var listsCourseRuns = new List<BulkUploadCourseRun>();

            foreach (var bulkUploadCourse in bulkUploadCourses)
            {
                if (bulkUploadCourse.IsCourseHeader)
                {
                    var course = new Course();
                    course.id = Guid.NewGuid();
                    course.QualificationCourseTitle = bulkUploadCourse.QualificationCourseTitle;
                    course.LearnAimRef = bulkUploadCourse.LearnAimRef;
                    course.NotionalNVQLevelv2 = bulkUploadCourse.NotionalNVQLevelv2;
                    course.AwardOrgCode = bulkUploadCourse.AwardOrgCode;
                    course.QualificationType = bulkUploadCourse.QualificationType;
                    course.ProviderUKPRN = bulkUploadCourse.ProviderUKPRN;
                    course.CourseDescription = bulkUploadCourse.CourseDescription;
                    course.EntryRequirements = bulkUploadCourse.EntryRequirements;
                    course.WhatYoullLearn = bulkUploadCourse.WhatYoullLearn;
                    course.HowYoullLearn = bulkUploadCourse.HowYoullLearn;
                    course.WhatYoullNeed = bulkUploadCourse.WhatYoullNeed;
                    course.HowYoullBeAssessed = bulkUploadCourse.HowYoullBeAssessed;
                    course.WhereNext = bulkUploadCourse.WhereNext;
                    course.AdvancedLearnerLoan = bulkUploadCourse.AdvancedLearnerLoan.Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                    course.AdultEducationBudget = bulkUploadCourse.AdultEducationBudget.Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                    course.BulkUploadErrors = ParseBulkUploadErrors(bulkUploadCourse.BulkUploadLineNumber, _courseService.ValidateCourse(course));
                    course.IsValid = course.BulkUploadErrors.Any() ? false : true;

                    course.CreatedBy = userId;
                    course.CreatedDate = DateTime.Now;

                    course.UpdatedBy = bulkUploadCourse.TempCourseId.ToString();

                    courses.Add(course);
                }

                var courseRun = new CourseRun();
                courseRun.id = Guid.NewGuid();

                courseRun.DeliveryMode = GetValueFromDescription<DeliveryMode>(bulkUploadCourse.DeliveryMode);
                if (courseRun.DeliveryMode.Equals(DeliveryMode.Undefined))
                {
                    validationMessages.Add($"DeliveryMode is Undefined, because you have entered ( { bulkUploadCourse.DeliveryMode } ), Line { bulkUploadCourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadCourse.LearnAimRef }, ID = { bulkUploadCourse.ProviderCourseID }");
                }

                // Call VenueService and for VenueName get VenueId (GUID) (Applicable only for type ClassroomBased)

                if (string.IsNullOrEmpty(bulkUploadCourse.VenueName))
                {
                    validationMessages.Add($"NO Venue Name for Line { bulkUploadCourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadCourse.LearnAimRef }, ID = { bulkUploadCourse.ProviderCourseID }");
                }
                else
                {
                    //GetVenuesByPRNAndNameCriteria venueCriteria = new GetVenuesByPRNAndNameCriteria(bulkUploadCourse.ProviderUKPRN.ToString(), bulkUploadCourse.VenueName);
                    var venueResultCache = cachedVenues.Where(o => o.VenueName.ToLower() == bulkUploadCourse.VenueName.ToLower() && o.Status == VenueStatus.Live).ToList();

                    if (null != venueResultCache && venueResultCache.Count > 0)
                    {
                        //var venues = (IEnumerable<Venue>)venueResultCeche.Value.Value;
                        if (venueResultCache.Count().Equals(1))
                        {
                            if (venueResultCache.FirstOrDefault().Status.Equals(VenueStatus.Live))
                            {
                                courseRun.VenueId = new Guid(venueResultCache.FirstOrDefault().ID);
                            }
                            else
                            {
                                validationMessages.Add($"Venue is not LIVE (The status is { venueResultCache.FirstOrDefault().Status }) for VenueName { bulkUploadCourse.VenueName } - Line { bulkUploadCourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadCourse.LearnAimRef }, ID = { bulkUploadCourse.ProviderCourseID }");
                            }
                        }
                        else
                        {
                            validationMessages.Add($"We have obtained muliple Venues for { bulkUploadCourse.VenueName } - Line { bulkUploadCourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadCourse.LearnAimRef }, ID = { bulkUploadCourse.ProviderCourseID }");
                            if (venueResultCache.FirstOrDefault().Status.Equals(VenueStatus.Live))
                            {
                                courseRun.VenueId = new Guid(venueResultCache.FirstOrDefault().ID);
                            }
                            else
                            {
                                validationMessages.Add($"The selected Venue is not LIVE (The status is { venueResultCache.FirstOrDefault().Status }) for VenueName { bulkUploadCourse.VenueName } - Line { bulkUploadCourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadCourse.LearnAimRef }, ID = { bulkUploadCourse.ProviderCourseID }");
                            }
                        }
                    }
                    else
                    {
                        validationMessages.Add($"We could NOT obtain a Venue for { bulkUploadCourse.VenueName } - Line { bulkUploadCourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadCourse.LearnAimRef }, ID = { bulkUploadCourse.ProviderCourseID }");
                    }
                }


                courseRun.CourseName = bulkUploadCourse.CourseName;
                courseRun.ProviderCourseID = bulkUploadCourse.ProviderCourseID;

                courseRun.FlexibleStartDate = bulkUploadCourse.FlexibleStartDate.Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;

                DateTime specifiedStartDate;
                if (DateTime.TryParseExact(bulkUploadCourse.StartDate, "dd/MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out specifiedStartDate))
                {
                    courseRun.StartDate = specifiedStartDate;
                }
                else if(DateTime.TryParse(bulkUploadCourse.StartDate, out specifiedStartDate))
                {
                    //Remove time
                    specifiedStartDate = specifiedStartDate.Date;
                    courseRun.StartDate = specifiedStartDate;
                }
                else
                {
                    courseRun.StartDate = null;
                    validationMessages.Add($"StartDate is NULL, because you have entered ( { bulkUploadCourse.StartDate } ), Line { bulkUploadCourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadCourse.LearnAimRef }, ID = { bulkUploadCourse.ProviderCourseID }. We are expecting the date in 'dd/MM/yyyy' format.");
                }

                courseRun.CourseURL = bulkUploadCourse.CourseURL;

                decimal specifiedCost;
                if (decimal.TryParse(bulkUploadCourse.Cost, out specifiedCost))
                {
                    courseRun.Cost = specifiedCost;
                }
                else
                {
                    courseRun.Cost = null;
                    validationMessages.Add($"Cost is NULL, because you have entered ( { bulkUploadCourse.Cost } ), Line { bulkUploadCourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadCourse.LearnAimRef }, ID = { bulkUploadCourse.ProviderCourseID }");
                }

                courseRun.CostDescription = ReplaceSpecialCharacters(bulkUploadCourse.CostDescription);
                courseRun.DurationUnit = GetValueFromDescription<DurationUnit>(bulkUploadCourse.DurationUnit);
                if (courseRun.DurationUnit.Equals(DurationUnit.Undefined))
                {
                    validationMessages.Add($"DurationUnit is Undefined, because you have entered ( { bulkUploadCourse.DurationUnit } ), Line { bulkUploadCourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadCourse.LearnAimRef }, ID = { bulkUploadCourse.ProviderCourseID }");
                }

                int specifiedDurationValue;
                if (int.TryParse(bulkUploadCourse.DurationValue, out specifiedDurationValue))
                {
                    courseRun.DurationValue = specifiedDurationValue;
                }
                else
                {
                    courseRun.DurationValue = null;
                    validationMessages.Add($"DurationValue is NULL, because you have entered ( { bulkUploadCourse.DurationValue } ), Line { bulkUploadCourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadCourse.LearnAimRef }, ID = { bulkUploadCourse.ProviderCourseID }");
                }

                courseRun.StudyMode = GetValueFromDescription<StudyMode>(bulkUploadCourse.StudyMode);
                if (courseRun.StudyMode.Equals(StudyMode.Undefined))
                {
                    validationMessages.Add($"StudyMode is Undefined, because you have entered ( { bulkUploadCourse.StudyMode } ), Line { bulkUploadCourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadCourse.LearnAimRef }, ID = { bulkUploadCourse.ProviderCourseID }");
                }
                courseRun.AttendancePattern = GetValueFromDescription<AttendancePattern>(bulkUploadCourse.AttendancePattern);
                if (courseRun.AttendancePattern.Equals(AttendancePattern.Undefined))
                {
                    validationMessages.Add($"AttendancePattern is Undefined, because you have entered ( { bulkUploadCourse.AttendancePattern } ), Line { bulkUploadCourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadCourse.LearnAimRef }, ID = { bulkUploadCourse.ProviderCourseID }");
                }

                switch(bulkUploadCourse.National.ToUpperInvariant())
                {
                    case "YES":
                        {
                            courseRun.National = true;
                            var availableRegions = new SelectRegionModel();
                            courseRun.Regions = availableRegions.RegionItems.Select(x => x.Id).ToList();
                            break;
                        }
                    case "NO":
                        {
                            courseRun.National = false;
                            var regionResult = ParseRegionData(bulkUploadCourse.Regions, bulkUploadCourse.SubRegions);
                            if(regionResult.IsSuccess && regionResult.HasValue)
                            {
                                courseRun.Regions = regionResult.Value;
                            }
                            else if (!regionResult.IsSuccess)
                            {
                                validationMessages.Add($"Unable to get regions/subregions, Line { bulkUploadCourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadCourse.LearnAimRef }, ID = { bulkUploadCourse.ProviderCourseID }");
                            }
                            break;
                        }
                    default:
                        {
                            courseRun.National = null;
                            validationMessages.Add($"Choose if you can deliver this course anywhere in England, Line { bulkUploadCourse.BulkUploadLineNumber },  LARS_QAN = { bulkUploadCourse.LearnAimRef }, ID = { bulkUploadCourse.ProviderCourseID }");
                            break;
                        }
                }


                courseRun.BulkUploadErrors = ParseBulkUploadErrors(bulkUploadCourse.BulkUploadLineNumber, _courseService.ValidateCourseRun(courseRun, ValidationMode.BulkUploadCourse));
                courseRun.RecordStatus = courseRun.BulkUploadErrors.Any() ? RecordStatus.BulkUploadPending : RecordStatus.BulkUploadReadyToGoLive;

                courseRun.CreatedBy = userId;
                courseRun.CreatedDate = DateTime.Now;

                listsCourseRuns.Add(new BulkUploadCourseRun { LearnAimRef = bulkUploadCourse.LearnAimRef, TempCourseId = bulkUploadCourse.TempCourseId, CourseRun = courseRun });
            }

            foreach (var course in courses)
            {
                int currentTempCourseId;
                if (int.TryParse(course.UpdatedBy, out currentTempCourseId))
                {
                    course.CourseRuns = listsCourseRuns.Where(cr => cr.LearnAimRef == course.LearnAimRef && cr.TempCourseId == currentTempCourseId).Select(cr => cr.CourseRun).ToList();
                }
                else
                {
                    validationMessages.Add($"Problem with parsing TempCourseId -  ( { course.UpdatedBy } ), LARS_QAN = { course.LearnAimRef }, CourseFor = { course.CourseDescription }");
                }

                course.UpdatedBy = null;
            }

            //// Uncomment only for DEV and TESTING
            //int courseNumber = 1;
            //foreach (var course in courses)
            //{
            //    string jsonBulkUploadCoursesFilesPath = "D:\\FindACourse-BulkUploadJSONfiles";
            //    var courseJson = JsonConvert.SerializeObject(course);
            //    string jsonFileName = string.Format("{0}-{1}-{2}-{3}-{4}.json", DateTime.Now.ToString("yyMMdd-HHmmss"), course.ProviderUKPRN, course.LearnAimRef, courseNumber, course.CourseRuns.Count().ToString());
            //    File.WriteAllText(string.Format(@"{0}\{1}", jsonBulkUploadCoursesFilesPath, jsonFileName), courseJson);
            //    courseNumber++;
            //}

            // Push the courses to the CourseService
            foreach (var course in courses)
            {
                var courseResult = Task.Run(async () => await _courseService.AddCourseAsync(course)).Result;

                if (courseResult.IsSuccess && courseResult.HasValue)
                {
                    // Do nothing. Eventually we could have a count on successfully uploaded courses
                }
                else
                {
                    errors.Add($"The course is NOT BulkUploaded, LARS_QAN = { course.LearnAimRef }. Error -  { courseResult.Error }");
                }
            }

            return courses;
        }
        public string ReplaceSpecialCharacters(string value)
        {
            var replacedString = value.Replace("â€™", "'").Replace("â€“", "–").Replace("�", "£");
            return replacedString;
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

                var alternativeName = Attribute.GetCustomAttribute(field, typeof(AlternativeNameAttribute)) as AlternativeNameAttribute;

                if (attribute != null)
                {
                    if (attribute.Description.Equals(description, StringComparison.InvariantCultureIgnoreCase))
                        return (T)field.GetValue(null);
                }
                if (alternativeName != null)
                {
                    if (alternativeName.Value.Equals(description, StringComparison.InvariantCultureIgnoreCase))
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name.Equals(description, StringComparison.InvariantCultureIgnoreCase))
                        return (T)field.GetValue(null);
                }
            }

            return default(T);
        }

        private IEnumerable<BulkUploadError> ParseBulkUploadErrors(int lineNumber, IList<KeyValuePair<string,string>> errors)
        {
            List<BulkUploadError> errorList = new List<BulkUploadError>();

            foreach(var error in errors)
            {
                //If non-bulk upload error
                if (error.Key == "NULL")
                {
                    continue;
                }
                BulkUploadError buError = new BulkUploadError
                {
                    LineNumber = lineNumber,
                    Header = error.Key,
                    Error = error.Value
                };
                errorList.Add(buError);
            }
            return errorList;
        }

        private Result<IEnumerable<string>> ParseRegionData(string regions, string subRegions)
        {
            List<string> totalList = new List<string>();

            var availableRegions = new SelectRegionModel();
            var availableSubRegions = availableRegions.RegionItems.SelectMany(x => x.SubRegion);
            var listOfRegions = regions.Split(";").Select(p => p.Trim()).ToList();
            var listOfSubregions = subRegions.Split(";").Select(p => p.Trim()).ToList();
            //Get regions
            foreach (var region in listOfRegions)
            {
                if (!string.IsNullOrWhiteSpace(region))
                {
                    var id = availableRegions.RegionItems.Where(x => x.RegionName.ToUpper() == region.ToUpper())
                                                     .Select(y => y.Id);
                    if (id.Count() > 0)
                    {
                        totalList.Add(id.FirstOrDefault());
                    }
                    else
                    {
                        return Result.Fail<IEnumerable<string>>("Problem with Bulk upload value");
                    }
                }
            }
            foreach(var subRegion in listOfSubregions)
            {
                if(!string.IsNullOrEmpty(subRegion))
                {
                    var id = availableSubRegions.Where(x => x.SubRegionName.ToUpper() == subRegion.ToUpper())
                                            .Select(y => y.Id);
                    if (id.Count() > 0)
                    {
                        totalList.Add(id.FirstOrDefault());
                    }
                    else
                    {
                        return Result.Fail<IEnumerable<string>>("Problem with Bulk upload value");
                    }
                }
            }
            return Result.Ok<IEnumerable<string>>(totalList);
        }
    }
}
