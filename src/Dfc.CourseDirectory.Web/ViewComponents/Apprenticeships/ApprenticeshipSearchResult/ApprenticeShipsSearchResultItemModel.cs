using System;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships.ApprenticeshipSearchResult
{
    public class ApprenticeShipsSearchResultItemModel
    {
        public string ApprenticeshipTitle { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public int? StandardCode { get; set; }
        public int? Version { get; set; }
        public bool OtherBodyApprovalRequired { get; set; }
        public ApprenticeshipType ApprenticeshipType { get; set; }
    }
}
