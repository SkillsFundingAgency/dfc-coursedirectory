using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using System;

namespace Dfc.CourseDirectory.Web.ViewModels.EditApprenticeship
{
    public class EditApprenticeshipViewModel
    {
        public Guid? ApprenticeshipId { get; set; }

        public string WebSite { get; set; }

        public string Email { get; set; }

        public string Telephone { get; set; }

        public string ContactUsURL { get; set; }

        public PublishMode Mode { get; set; }

        public string ApprenticeshipTitle { get; set; }

        public int? StandardCode { get; set; }
        public int? FrameworkCode { get; set; }

        public int? ProgType { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public int? Version { get; set; }
        public int? PathwayCode { get; set; }
        public ApprenticeshipType ApprenticeshipType { get; set; }
        public ApprenticeShipPreviousPage ApprenticeshipPreviousPage { get; set; }
        public string Information { get; set; }
        public bool? Cancelled { get; set; }
        public bool? ShowCancelled { get; set; }
    }
}