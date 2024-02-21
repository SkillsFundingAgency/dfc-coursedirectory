using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class ProviderCampaignCode
    {
        public string CodeId { get; set; }
        public Guid ProviderId { get; set; }
        public string LearnAimRef { get; set; }
        public string CampaignCodes { get; set; }
    }
}
