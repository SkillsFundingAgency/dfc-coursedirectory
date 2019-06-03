
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using System;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class ApprenticeShipDetailsRequestModel
    {
        public Guid Id { get; set; }
        public int? StandardCode { get; set; }
        public int? FrameworkCode { get; set; }
        public int? Version { get; set; }
        public int? PathwayCode { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public string NotionalEndLevel { get; set; }
        public ApprenticeshipType ApprenticeshipType { get; set; }
        public int? ProgType { get; set; }
        public ApprenticeShipPreviousPage PreviousPage { get; set; }
       
    }
}
