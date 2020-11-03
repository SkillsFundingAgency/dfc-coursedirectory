using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Web.ViewComponents.ProviderApprenticeships.ProviderApprenticeshipSearchResult
{
    public class ProviderApprenticeShipsSearchResultItemModel
    {
        public string ApprenticeshipTitle { get; set; }

        public string ApprenticeshipType { get; set; }
        //public string LearnAimRef { get; }
        //public string LearnAimRefTitle { get; }
        public string NotionalNVQLevelv2 { get; set; }
        //public string AwardOrgCode { get; }
        //public string LearnDirectClassSystemCode1 { get; }
        //public string LearnDirectClassSystemCode2 { get; }
        //public string GuidedLearningHours { get; }
        //public string TotalQualificationTime { get; }
        //public string UnitType { get; }
        //public string AwardOrgName { get; }
        //public string LearnAimRefTypeDesc { get; }
        //public DateTime? CertificationEndDate { get; }

       
    }
}