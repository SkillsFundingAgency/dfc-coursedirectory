using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Web.ViewModels;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class ProviderBriefOverviewRequestModel
    {
        public string UKPRN { get; set; }
        public string BriefOverview { get; set; }
       
    }
}
