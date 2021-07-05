using System;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class RemoveStaleCampaignProviderCourses : ISqlQuery<Success>
    {
        public string CampaignCode { get; set; }
        public Guid ImportJobId { get; set; }
    }
}
