using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Apprenticeships
{
    public interface ICsvApprenticeship
    {
        string StandardCode { get; set; }
        string Version { get; set; }
        string FrameworkCode { get; set; }
        string ProgType { get; set; }
        string PathwayCode { get; set; }
        string ApprenticeshipInformation { get; set; }
        string ApprenticeshipWebpage { get; set; }
        string ContactEmail { get; set; }
        string ContactPhone { get; set; }
        string ContactURL { get; set; }
        string DeliveryMethod { get; set; }
        string Venue { get; set; }
        string Radius { get; set; }
        string DeliveryMode { get; set; }
        string AcrossEngland { get; set; }
        string NationalDelivery { get; set; }
        string Region { get; set; }
        string Subregion { get; set; }
    }
}
