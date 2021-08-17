using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Web.Models.Apprenticeships;

namespace Dfc.CourseDirectory.Web.ViewModels.Apprenticeships
{
    public class DetailViewModel
    {
        public int StandardCode { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public int Version { get; set; }
        public ApprenticeshipType ApprenticeshipType { get; set; }
        public ApprenticeShipPreviousPage ApprenticeshipPreviousPage { get; set; }
        public string Information { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string ContactUsIUrl { get; set; }
        public ApprenticeshipMode Mode { get; set; }
        public bool? Cancelled { get; set; }
        public bool? ShowCancelled { get; set; }
    }
}
