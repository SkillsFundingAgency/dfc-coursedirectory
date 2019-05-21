
using Dfc.CourseDirectory.Models.Enums;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class ApprenticeShipDetailsRequestModel
    {
        public string ApprenticeshipTitle { get; set; }

        public ApprenticeShipMode ApprenticeshipMode { get; set; }
        
        public ApprenticeShipPreviousPage PreviousPage { get; set; }
       
    }
}
