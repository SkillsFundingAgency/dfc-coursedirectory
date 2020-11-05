
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Apprenticeships;
using System;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class DetailsRequestModel
    {
        public Guid Id { get; set; }
        public int? StandardCode { get; set; }
        public int? FrameworkCode { get; set; }
        public int? Version { get; set; }
        public int? PathwayCode { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public ApprenticeshipType ApprenticeshipType { get; set; }
        public int? ProgType { get; set; }
        public ApprenticeShipPreviousPage PreviousPage { get; set; }

        public ApprenticeshipMode Mode { get; set; }

        public bool? Cancelled { get; set; }

        public bool? ShowCancelled { get; set; }

    }
}
