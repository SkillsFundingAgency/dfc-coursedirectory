using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class SetCampaignProviderCourses : ISqlQuery<Success>
    {
        public string CampaignCode { get; set; }
        public IEnumerable<SetCampaignProviderCoursesRecord> Records { get; set; }
    }

    public class SetCampaignProviderCoursesRecord
    {
        public int ProviderUkprn { get; set; }
        public string LearnAimRef { get; set; }
    }
}
