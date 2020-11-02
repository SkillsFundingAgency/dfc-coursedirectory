using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Web.ViewModels;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class ProviderAddOrEditRequestModel
    {
        public string UKPRN { get; set; }
        public string Alias { get; set; }

        public string BriefOverview { get; set; }

    }
}
