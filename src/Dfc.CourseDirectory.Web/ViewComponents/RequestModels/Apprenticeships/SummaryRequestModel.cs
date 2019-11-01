
using Dfc.CourseDirectory.Models.Models.Apprenticeships;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class SummaryRequestModel
    {
        public string Id { get; set; }

        public ApprenticeshipMode Mode { get; set; }

        public bool SummaryOnly { get; set; }
       
    }
}
