using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models
{
    public class CourseMigrationReport
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        public IList<Course> LarslessCourses { get; set; }
        public int PreviousLiveCourseCount { get; set; }
        public int ProviderUKPRN { get; set; }
    }
}
