using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Web.Models.Apprenticeships;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class DetailsRequestModel
    {
        public int StandardCode { get; set; }
        public int Version { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public ApprenticeShipPreviousPage PreviousPage { get; set; }
        public ApprenticeshipMode Mode { get; set; }
        public bool? Cancelled { get; set; }
        public bool? ShowCancelled { get; set; }
    }
}
