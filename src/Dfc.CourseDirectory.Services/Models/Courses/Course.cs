﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Services.Models.Courses
{
    public class Course : ICloneable
    {
        public Guid id { get; set; }
        public int? CourseId { get; set; }
        public string QualificationCourseTitle { get; set; }
        public string LearnAimRef { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string AwardOrgCode { get; set; }
        public string QualificationType { get; set; }
        public int ProviderUKPRN { get; set; }
        public string CourseDescription { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string WhatYoullNeed { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhereNext { get; set; }
        public bool AdultEducationBudget { get; set; }
        public bool AdvancedLearnerLoan { get; set; }
        public IEnumerable<CourseRun> CourseRuns { get; set; }
        public bool IsValid { get; set; }
        public IEnumerable<string> ValidationErrors { get; set; }
        public IEnumerable<BulkUploadError> BulkUploadErrors { get; set; }
        public RecordStatus CourseStatus => GetBitMaskState(CourseRuns);
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }

        public Course Clone() {
            return (Course)this.MemberwiseClone();
        }

        object ICloneable.Clone() {
            return Clone();
        }

        private static RecordStatus GetBitMaskState(IEnumerable<CourseRun> courseRuns)
        {
            RecordStatus courseStatus = RecordStatus.Undefined; // Default BitMaskState (handles undefined and no CourseRuns)

            if (courseRuns != null)
            {
                foreach (RecordStatus recordStatus in Enum.GetValues(typeof(RecordStatus)))
                {
                    if (courseRuns.Any(c => c.RecordStatus == recordStatus))
                    {
                        BitmaskHelper.Set(ref courseStatus, recordStatus);
                    }
                }
            }
            return courseStatus;
        }
    }
}
