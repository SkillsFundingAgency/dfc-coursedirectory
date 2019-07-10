
using Dfc.CourseDirectory.Models.Models.Apprenticeships;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class SummaryRequestModel
    {
        public string Id { get; set; }

        public ApprenticeshipMode apprenticeshipMode { get; set; }

        public bool cancelled { get; set; }
       
    }
}
