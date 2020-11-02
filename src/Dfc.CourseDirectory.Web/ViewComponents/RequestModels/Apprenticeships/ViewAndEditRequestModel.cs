
using Dfc.CourseDirectory.Services.Models.Apprenticeships;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class ViewAndEditRequestModel
    {
        public string Id { get; set; }

        public ApprenticeshipMode Mode { get; set; }

        public bool? cancelled { get; set; }
       
    }
}
