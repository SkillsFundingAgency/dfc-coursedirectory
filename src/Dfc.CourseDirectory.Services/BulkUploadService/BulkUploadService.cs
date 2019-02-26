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

namespace Dfc.CourseDirectory.Services.BulkUploadService
{
    public class BulkUploadService : IBulkUploadService
    {
        private readonly ILogger<BulkUploadService> _logger;
        private readonly ILarsSearchSettings _larsSearchSettings;
        private readonly ILarsSearchService _larsSearchService;

        public BulkUploadService(
            ILogger<BulkUploadService> logger,
            IOptions<LarsSearchSettings> larsSearchSettings,
            ILarsSearchService larsSearchService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(larsSearchSettings, nameof(larsSearchSettings));
            Throw.IfNull(larsSearchService, nameof(larsSearchService));

            _logger = logger;
            _larsSearchSettings = larsSearchSettings.Value;
            _larsSearchService = larsSearchService;
        }

        public List<string> ProcessBulkUpload(string bulkUploadFilePath, int providerUKPRN)
        {
            var errors = new List<string>();
            var courses = new List<BulkUploadCourse>();
            int bulkUploadLineNumber = 1;
            string previousLearnAimRef = string.Empty;
            List<int> lineNumbersInCourse = new List<int>();

            using (var reader = new StreamReader(bulkUploadFilePath))
            {
                using (var csv = new CsvReader(reader))
                {
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        bool isCourseHeader = false;
                        string currentLearnAimRef = csv.GetField("LARS_QAN");
                        if (bulkUploadLineNumber.Equals(1) || currentLearnAimRef != previousLearnAimRef)
                        {
                            isCourseHeader = true;
                            //lineNumbersInCourse.Add(bulkUploadLineNumber);
                        }
                        //else
                        //{
                        //    previousLearnAimRef = currentLearnAimRef;
                        //}

                        var record = new BulkUploadCourse
                        {
                            IsCourseHeader = isCourseHeader,
                            BulkUploadLineNumber = bulkUploadLineNumber,
                            LearnAimRef = currentLearnAimRef,
                            ProviderUKPRN = providerUKPRN,
                            VenueName = csv.GetField("VENUE"),
                            CourseName = csv.GetField("COURSE_NAME"),
                            ProviderCourseID = csv.GetField("ID"),
                            DeliveryMode = csv.GetField("DELIVERY_MODE"),
                            FlexibleStartDate = csv.GetField("FLEXIBLE_START_DATE"),
                            StartDate = csv.GetField("START_DATE"),
                            CourseURL = csv.GetField("URL"),
                            Cost = csv.GetField("COST"),
                            CostDescription = csv.GetField("COST_DESCRIPTION"),
                            DurationUnit = csv.GetField("DURATION_UNIT"),
                            DurationValue = csv.GetField("DURATION"),
                            StudyMode = csv.GetField("STUDY_MODE"),
                            AttendancePattern = csv.GetField("ATTENDANCE_PATTERN")
                        };

                        if (isCourseHeader)
                        {
                            record.CourseDescription = csv.GetField("WHO_IS_THIS_COURSE_FOR");
                            record.EntryRequirements = csv.GetField("ENTRY_REQUIREMENTS");
                            record.WhatYoullLearn = csv.GetField("WHAT_YOU_WILL_LEARN");
                            record.HowYoullLearn = csv.GetField("HOW_YOU_WILL_LEARN");
                            record.WhatYoullNeed = csv.GetField("WHAT_YOU_WILL_NEED_TO_BRING");
                            record.HowYoullBeAssessed = csv.GetField("HOW_YOU_WILL_BE_ASSESSED");
                            record.WhereNext = csv.GetField("WHERE_NEXT");
                            record.AdultEducationBudget = csv.GetField("ADULT_EDUCATION_BUDGET"); // bool
                            record.AdvancedLearnerLoan = csv.GetField("ADVANCED_LEARNER_OPTION");
                        }

                        courses.Add(record);

                        previousLearnAimRef = currentLearnAimRef;
                        bulkUploadLineNumber++;
                    }
                }
            }

            courses = PolulateLARSData(courses, out errors);


            return errors;
        }

        public List<BulkUploadCourse> PolulateLARSData(List<BulkUploadCourse> courses, out List<string> errors)
        {
            errors = new List<string>();

            foreach (var course in courses.Where(c => c.IsCourseHeader == true).ToList())
            {
                LarsSearchCriteria criteria = new LarsSearchCriteria(course.LearnAimRef, 10, 0, string.Empty, null);
                var larsResult = Task.Run(async () => await _larsSearchService.SearchAsync(criteria)).Result;

                if (larsResult.IsSuccess && larsResult.HasValue)
                {
                    var qualifications = new List<LarsDataResultItem>();
                    foreach (var item in larsResult.Value.Value)
                    {
                        var larsDataResultItem = new LarsDataResultItem
                        {
                            LearnAimRefTitle = item.LearnAimRefTitle,
                            NotionalNVQLevelv2 = item.NotionalNVQLevelv2,
                            AwardOrgCode = item.AwardOrgCode,
                            LearnAimRefTypeDesc = item.LearnAimRefTypeDesc
                        };
                        qualifications.Add(larsDataResultItem);
                    }

                    if (qualifications.Count.Equals(0))
                    {
                        List<int> invalidLARSLineNumbers = courses.Where(c => c.LearnAimRef == course.LearnAimRef).Select(l => l.BulkUploadLineNumber).ToList();
                        errors.Add($"{ InvalidLARSLineNumbersToString(invalidLARSLineNumbers) }, LARS_QAN = { course.LearnAimRef } invalid LARS");
                        //errors.Add($"Line { course.BulkUploadLineNumber }, LARS_QAN = { course.LearnAimRef } invalid LARS");
                    }
                    else if (qualifications.Count.Equals(1))
                    {
                        course.QualificationCourseTitle = qualifications[0].LearnAimRefTitle;
                        course.NotionalNVQLevelv2 = qualifications[0].NotionalNVQLevelv2;
                        course.AwardOrgCode = qualifications[0].AwardOrgCode;
                        course.QualificationType = qualifications[0].LearnAimRefTypeDesc;
                    }
                    else
                    {
                        string logMoreQualifications = string.Empty;
                        foreach (var qualification in qualifications)
                        {
                            logMoreQualifications += "( '" + qualification.LearnAimRefTitle + "' with Level " + qualification.NotionalNVQLevelv2 + " and AwardOrgCode " + qualification.AwardOrgCode + " ) ";
                        }
                        errors.Add($"We retrieve multiple qualifications ( { qualifications.Count.ToString() } ) for the LARS { course.LearnAimRef }, which are { logMoreQualifications } ");
                    }
                }
                else
                {
                    errors.Add($"We couldn't retreive LARS data for LARS { course.LearnAimRef }, because of technical reason, Error: " + larsResult?.Error);
                }
            }

            return courses;
        }

        public string InvalidLARSLineNumbersToString(List<int> invalidLARSLineNumbers)
        {
            string invalidLARSLineNumbersToString = string.Empty;

            if (invalidLARSLineNumbers.Count.Equals(1))
                invalidLARSLineNumbersToString = $"Line { invalidLARSLineNumbers[0] }";
            if(invalidLARSLineNumbers.Count > 1)
            {
                int lastNumber = invalidLARSLineNumbers[invalidLARSLineNumbers.Count - 1];
                invalidLARSLineNumbers.RemoveAt(invalidLARSLineNumbers.Count - 1);
                invalidLARSLineNumbersToString = "Lines ";
                invalidLARSLineNumbersToString += string.Join(", ", invalidLARSLineNumbers);
                invalidLARSLineNumbersToString += " and " + lastNumber;
            }

            return invalidLARSLineNumbersToString;
        }
    }
}
