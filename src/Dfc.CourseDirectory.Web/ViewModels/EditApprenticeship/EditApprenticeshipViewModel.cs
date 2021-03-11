using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Web.Models.Apprenticeships;

namespace Dfc.CourseDirectory.Web.ViewModels.EditApprenticeship
{
    public class EditApprenticeshipViewModel
    {
        public Guid? ApprenticeshipId { get; set; }
        public string WebSite { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string ContactUsURL { get; set; }
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
        public IEnumerable<Apprenticeship> ListOfApprenticeships { get; set; }
        public int NumberOfApprenticeships { get; set; }
        public bool AreAllReadyToBePublished { get; set; }
        public string NotificationTitle { get; set; }
        public string NotificationMessage { get; set; }
        public Guid? LocationId { get; set; }
        public bool DayRelease { get; set; }
        public bool BlockRelease { get; set; }
        public int? Radius { get; set; }
        public bool? National { get; set; }
        public ApprenticeshipMode Mode { get; set; }
        public List<ApprenticeshipLocation> locations { get; set; }
        public bool HasOtherDeliveryOptions { get; set; }
    }
}
