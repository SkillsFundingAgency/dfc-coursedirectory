using System;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models
{
    public class CourseText
    {
        [JsonProperty("id")]
        public Guid CourseTextId { get; set; }
        public string LearnAimRef { get; set; }
        public string QualificationCourseTitle { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string TypeName { get; set; }
        public string AwardOrgCode { get; set; }
        public string CourseDescription { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string WhatYoullNeed { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhereNext { get; set; }
    }
}
