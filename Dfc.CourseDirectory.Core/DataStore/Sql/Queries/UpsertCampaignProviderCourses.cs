using System;
using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertCampaignProviderCourses : ISqlQuery<Success>
    {
        public string CampaignCode { get; set; }
        public Guid ImportJobId { get; set; }
        public IEnumerable<UpsertCampaignProviderCoursesRecord> Records { get; set; }
    }

    public class UpsertCampaignProviderCoursesRecord
    {
        public int ProviderUkprn { get; set; }
        public string LearnAimRef { get; set; }
    }
}
